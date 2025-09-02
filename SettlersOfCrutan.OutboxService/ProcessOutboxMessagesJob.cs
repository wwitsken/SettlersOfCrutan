using Microsoft.Extensions.Options;
using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Infrastructure.Outbox;
using SettlersOfCrutan.Infrastructure.Redis;
using SettlersOfCrutan.Infrastructure.Redis.Serialization;
using StackExchange.Redis;
using System.Text.Json;

namespace SettlersOfCrutan.OutboxService;

// Processes domain events from Redis Streams using consumer groups with:
// - Blocking reads (XREADGROUP ... BLOCK) for low-latency event-driven consumption
// - Retries via PEL + XAUTOCLAIM with attempts tracked in a Redis hash
// - Poison-queue after exceeding max attempts
public class ProcessOutboxMessagesJob(IDomainEventPublisher publisher, IOptions<RedisOptions> options)
{
    private readonly IDomainEventPublisher _publisher = publisher;
    private readonly RedisOptions _options = options?.Value ?? new RedisOptions();

    private const string Group = "outbox-consumers";
    private readonly string _consumer = $"worker-{Environment.MachineName}-{Environment.ProcessId}";

    private const int BatchSize = 50;
    private const int MaxAttempts = 5;

    private const int BlockMs = 4_000;   // XREADGROUP BLOCK time (must be < client command timeout)
    private const int AutoClaimIdleMs = 60_000; // idle time before claiming from PEL
    private const int AutoClaimBatch = 100;

    public async Task Execute(IDatabase db)
    {
        ArgumentNullException.ThrowIfNull(db);

        var outbox = RedisKeys.Outbox(_options.KeyPrefix);
        var poison = RedisKeys.OutboxPoison(_options.KeyPrefix);
        var attemptsHash = RedisKeys.OutboxAttemptsHash(_options.KeyPrefix);

        await EnsureGroupAsync(db, outbox).ConfigureAwait(false);

        // Main loop: block for new messages, then re-process idle PEL entries
        while (true)
        {
            var newEntries = await ReadNewAsync(db, outbox).ConfigureAwait(false);
            if (newEntries.Length > 0)
            {
                foreach (var entry in newEntries)
                    await ProcessNewAsync(db, outbox, poison, entry).ConfigureAwait(false);
            }

            await ReprocessIdlePendingAsync(db, outbox, poison, attemptsHash).ConfigureAwait(false);
        }
    }

    // --- High-level steps ----------------------------------------------------

    private static async Task EnsureGroupAsync(IDatabase db, RedisKey stream)
    {
        try
        {
            await db.StreamCreateConsumerGroupAsync(stream, Group, position: "$", createStream: true).ConfigureAwait(false);
        }
        catch (RedisServerException ex) when (ex.Message.Contains("BUSYGROUP", StringComparison.OrdinalIgnoreCase))
        {
            // already exists
        }
    }

    private async Task<StreamEntry[]> ReadNewAsync(IDatabase db, RedisKey stream)
    {
        // XREADGROUP GROUP <group> <consumer> COUNT <n> BLOCK <ms> STREAMS <stream> >
        var args = new object?[] { "GROUP", Group, _consumer, "BLOCK", BlockMs, "COUNT", BatchSize, "STREAMS", stream, ">" };
        var result = await db.ExecuteAsync("XREADGROUP", args).ConfigureAwait(false);
        if (result.IsNull) return [];
        return ParseXReadResult(result);
    }

    private async Task ProcessNewAsync(IDatabase db, RedisKey stream, RedisKey poison, StreamEntry entry)
    {
        var id = entry.Id;

        if (!TryGetEnvelope(entry, out var envelope, out var reason))
        {
            await DeadLetterAsync(db, poison, id, reason!, GetField(entry, "payload")).ConfigureAwait(false);
            await AckAndDeleteAsync(db, stream, id).ConfigureAwait(false);
            return;
        }

        if (!TryDeserializeEvent(envelope, out var domainEvent, out var badType))
        {
            await DeadLetterAsync(db, poison, id, $"UnknownEventType:{badType}", envelope.Payload).ConfigureAwait(false);
            await AckAndDeleteAsync(db, stream, id).ConfigureAwait(false);
            return;
        }

        try
        {
            await _publisher.PublishAsync(domainEvent!);
            await AcknowledgeAsync(db, stream, id).ConfigureAwait(false);
        }
        catch
        {
            // Leave un-ACKed -> remains in PEL for later XAUTOCLAIM
        }
    }

    private async Task ReprocessIdlePendingAsync(IDatabase db, RedisKey stream, RedisKey poison, RedisKey attemptsHash)
    {
        var entries = await AutoClaimAsync(db, stream).ConfigureAwait(false);
        if (entries.Length == 0) return;

        foreach (var entry in entries)
        {
            var id = entry.Id;

            // Read attempts
            int attempts = 0;
            var attemptsVal = await db.HashGetAsync(attemptsHash, id).ConfigureAwait(false);
            if (attemptsVal.HasValue && int.TryParse(attemptsVal.ToString(), out var parsed)) attempts = parsed;

            if (attempts >= MaxAttempts)
            {
                await DeadLetterAsync(db, poison, id, "MaxRetriesExceeded", GetField(entry, "payload")).ConfigureAwait(false);
                await AckAndDeleteAsync(db, stream, id).ConfigureAwait(false);
                await db.HashDeleteAsync(attemptsHash, id).ConfigureAwait(false);
                continue;
            }

            // Try processing again
            try
            {
                if (!TryGetEnvelope(entry, out var env, out var reason))
                {
                    await DeadLetterAsync(db, poison, id, reason!, GetField(entry, "payload")).ConfigureAwait(false);
                    await AckAndDeleteAsync(db, stream, id).ConfigureAwait(false);
                    await db.HashDeleteAsync(attemptsHash, id).ConfigureAwait(false);
                    continue;
                }

                if (!TryDeserializeEvent(env, out var ev, out var badType))
                {
                    await DeadLetterAsync(db, poison, id, $"UnknownEventType:{badType}", env.Payload).ConfigureAwait(false);
                    await AckAndDeleteAsync(db, stream, id).ConfigureAwait(false);
                    await db.HashDeleteAsync(attemptsHash, id).ConfigureAwait(false);
                    continue;
                }

                await _publisher.PublishAsync(ev!).ConfigureAwait(false);

                await AcknowledgeAsync(db, stream, id).ConfigureAwait(false);
                await db.HashDeleteAsync(attemptsHash, id).ConfigureAwait(false);
            }
            catch
            {
                // Increment attempts and keep in PEL
                await db.HashIncrementAsync(attemptsHash, id, 1).ConfigureAwait(false);
            }
        }
    }

    // --- Redis wrappers -------------------------------------------------------

    private static StreamEntry[] ParseXReadResult(in RedisResult result)
    {
        // [ [ stream , [ [id, [field, value, ...]], ... ] ] ]
        try
        {
            var outer = (RedisResult[])result;
            if (outer is null || outer.Length == 0) return [];

            var list = new List<StreamEntry>();
            foreach (var streamRes in outer)
            {
                var streamArr = (RedisResult[])streamRes;
                if (streamArr is null || streamArr.Length < 2) continue;

                var entriesArr = (RedisResult[])streamArr[1];
                if (entriesArr is null || entriesArr.Length == 0) continue;

                foreach (var entryRes in entriesArr)
                {
                    var entryArr = (RedisResult[])entryRes;
                    if (entryArr is null || entryArr.Length < 2) continue;

                    var id = (string)entryArr[0];
                    var fieldsArr = (RedisResult[])entryArr[1];

                    var values = new List<NameValueEntry>();
                    for (int i = 0; i < fieldsArr.Length - 1; i += 2)
                    {
                        values.Add(new NameValueEntry((string)fieldsArr[i], (string)fieldsArr[i + 1]));
                    }
                    list.Add(new StreamEntry(id, [.. values]));
                }
            }
            return [.. list];
        }
        catch { return []; }
    }

    private static async Task<StreamEntry[]> AutoClaimAsync(IDatabase db, RedisKey stream)
    {
        // XAUTOCLAIM <stream> <group> <consumer> <min-idle> 0-0 COUNT <batch>
        var args = new object?[] { stream, Group, ConsumerStatic, AutoClaimIdleMs, "0-0", "COUNT", AutoClaimBatch };
        var result = await db.ExecuteAsync("XAUTOCLAIM", args).ConfigureAwait(false);
        if (result.IsNull) return [];
        return ParseXAutoClaimResult(result);
    }

    private static StreamEntry[] ParseXAutoClaimResult(in RedisResult result)
    {
        // [ nextStartId, [ [id, [field, value, ...]], ... ] ]
        try
        {
            var arr = (RedisResult[])result;
            if (arr is null || arr.Length < 2) return [];

            var entriesArr = (RedisResult[])arr[1];
            if (entriesArr is null || entriesArr.Length == 0) return [];

            var list = new List<StreamEntry>(entriesArr.Length);
            foreach (var entryRes in entriesArr)
            {
                var entryArr = (RedisResult[])entryRes;
                if (entryArr is null || entryArr.Length < 2) continue;

                var id = (string)entryArr[0];
                var fieldsArr = (RedisResult[])entryArr[1];

                var values = new List<NameValueEntry>();
                for (int i = 0; i < fieldsArr.Length - 1; i += 2)
                {
                    values.Add(new NameValueEntry((string)fieldsArr[i], (string)fieldsArr[i + 1]));
                }
                list.Add(new StreamEntry(id, [.. values]));
            }
            return [.. list];
        }
        catch { return []; }
    }

    private static async Task AcknowledgeAsync(IDatabase db, RedisKey stream, RedisValue id)
        => await db.StreamAcknowledgeAsync(stream, Group, id);

    private static async Task AckAndDeleteAsync(IDatabase db, RedisKey stream, RedisValue id)
    {
        await AcknowledgeAsync(db, stream, id).ConfigureAwait(false);
        await db.StreamDeleteAsync(stream, [id]).ConfigureAwait(false);
    }

    private static async Task DeadLetterAsync(IDatabase db, RedisKey poison, RedisValue id, string reason, string? payload)
        => await db.StreamAddAsync(poison, [new("origId", id), new("reason", reason), new("payload", payload ?? string.Empty)]);

    // --- Envelope + Event helpers --------------------------------------------

    private static bool TryGetEnvelope(in StreamEntry entry, out OutboxEnvelope? env, out string? reason)
    {
        env = null; reason = null;
        var payload = GetField(entry, "payload");
        if (string.IsNullOrWhiteSpace(payload)) { reason = "InvalidEnvelope"; return false; }
        try { env = JsonSerializer.Deserialize<OutboxEnvelope>(payload!, JsonOptions.Default); return env is not null; }
        catch { reason = "InvalidEnvelope"; return false; }
    }

    private static bool TryDeserializeEvent(OutboxEnvelope env, out IDomainEvent? ev, out string? badType)
    {
        ev = null; badType = null;
        var type = ResolveType(env.EventType);
        if (type is null) { badType = env.EventType; return false; }
        try { ev = JsonSerializer.Deserialize(env.Payload, type, JsonOptions.Default) as IDomainEvent; return ev is not null; }
        catch { badType = env.EventType; return false; }
    }

    private static string? GetField(in StreamEntry entry, string name)
    {
        foreach (var nve in entry.Values)
            if (nve.Name == name) return nve.Value;
        return null;
    }

    private static Type? ResolveType(string typeName)
    {
        var type = Type.GetType(typeName, throwOnError: false, ignoreCase: false);
        if (type is not null) return type;
        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            type = asm.GetType(typeName, throwOnError: false, ignoreCase: false);
            if (type is not null) return type;
        }
        return null;
    }

    // Needed for static usage in AutoClaimAsync arg array
    private static string ConsumerStatic => $"worker-{Environment.MachineName}-{Environment.ProcessId}";
}
