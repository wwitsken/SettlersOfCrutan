using Microsoft.Extensions.Options;
using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Infrastructure.Outbox;
using SettlersOfCrutan.Infrastructure.Redis;
using SettlersOfCrutan.Infrastructure.Redis.Serialization;
using StackExchange.Redis;
using System.Text.Json;

namespace SettlersOfCrutan.OutboxService;

// Processes domain events from the global outbox Redis Stream using intermittent polling.
// - Intended to be triggered by a scheduler (Quartz)
// - Tracks failures in a Redis hash for retries
// - Dead-letters after exceeding max attempts
public class ProcessOutboxMessagesJob(IDomainEventPublisher publisher, IOptions<RedisOptions> options)
{
    private readonly IDomainEventPublisher _publisher = publisher;
    private readonly RedisOptions _options = options?.Value ?? new RedisOptions();

    private const int BatchSize = 50;
    private const int MaxAttempts = 5;

    public async Task ExecuteOnce(IDatabase db, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(db);

        var outbox = RedisKeys.Outbox(_options.KeyPrefix);
        var poison = RedisKeys.OutboxPoison(_options.KeyPrefix);
        var attemptsHash = RedisKeys.OutboxAttemptsHash(_options.KeyPrefix);
        var cursorKey = GetCursorKey(_options.KeyPrefix);

        var lastId = await LoadCursorAsync(db, cursorKey, outbox);
        var entries = await ReadBatchAsync(db, outbox, lastId);

        foreach (var entry in entries)
        {
            ct.ThrowIfCancellationRequested();

            lastId = entry.Id;
            await ProcessEntryAsync(db, outbox, poison, attemptsHash, entry);
        }

        if (entries.Length > 0)
            await SaveCursorAsync(db, cursorKey, lastId);
    }

    // --- Cursor --------------------------------------------------------------

    private static RedisKey GetCursorKey(string prefix) => $"{prefix}:outbox:cursor";

    private static async Task<RedisValue> LoadCursorAsync(IDatabase db, RedisKey cursorKey, RedisKey outboxStream)
    {
        var existing = await db.StringGetAsync(cursorKey);
        if (existing.HasValue) return existing;

        var latest = await GetLatestIdAsync(db, outboxStream);
        await db.StringSetAsync(cursorKey, latest);
        return latest;
    }

    private static Task<bool> SaveCursorAsync(IDatabase db, RedisKey cursorKey, RedisValue lastId)
        => db.StringSetAsync(cursorKey, lastId);

    // --- Polling -------------------------------------------------------------

    private static async Task<RedisValue> GetLatestIdAsync(IDatabase db, RedisKey stream)
    {
        try
        {
            var info = await db.StreamInfoAsync(stream);
            return info.LastGeneratedId;
        }
        catch
        {
            return "0-0";
        }
    }

    private static async Task<StreamEntry[]> ReadBatchAsync(IDatabase db, RedisKey stream, RedisValue lastId)
    {
        var result = await db.StreamReadAsync(stream, lastId, BatchSize);
        return result ?? [];
    }

    // --- Processing ----------------------------------------------------------

    private async Task ProcessEntryAsync(IDatabase db, RedisKey stream, RedisKey poison, RedisKey attemptsHash, StreamEntry entry)
    {
        var id = entry.Id;

        if (!TryGetEnvelope(entry, out var envelope, out var badEnvelopeReason))
        {
            await DeadLetterAsync(db, poison, id, badEnvelopeReason!, GetField(entry, "payload"));
            await DeleteAsync(db, stream, id);
            await db.HashDeleteAsync(attemptsHash, id);
            return;
        }

        if (!TryDeserializeEvent(envelope, out var domainEvent, out var badType))
        {
            await DeadLetterAsync(db, poison, id, $"UnknownEventType:{badType}", envelope.Payload);
            await DeleteAsync(db, stream, id);
            await db.HashDeleteAsync(attemptsHash, id);
            return;
        }

        try
        {
            await _publisher.PublishAsync(domainEvent!);
            await DeleteAsync(db, stream, id);
            await db.HashDeleteAsync(attemptsHash, id);
        }
        catch
        {
            var attempts = await db.HashIncrementAsync(attemptsHash, id, 1);
            if (attempts >= MaxAttempts)
            {
                await DeadLetterAsync(db, poison, id, "MaxRetriesExceeded", GetField(entry, "payload"));
                await DeleteAsync(db, stream, id);
                await db.HashDeleteAsync(attemptsHash, id);
            }
        }
    }

    private static Task<long> DeleteAsync(IDatabase db, RedisKey stream, RedisValue id)
        => db.StreamDeleteAsync(stream, [id]);

    private static Task<RedisValue> DeadLetterAsync(IDatabase db, RedisKey poison, RedisValue id, string reason, string? payload)
        => db.StreamAddAsync(poison, [new("origId", id), new("reason", reason), new("payload", payload ?? string.Empty)]);

    // --- Envelope + Event helpers --------------------------------------------

    private static bool TryGetEnvelope(in StreamEntry entry, out OutboxEnvelope? env, out string? reason)
    {
        env = null;
        reason = null;

        var payload = GetField(entry, "payload");
        if (string.IsNullOrWhiteSpace(payload))
        {
            reason = "InvalidEnvelope";
            return false;
        }

        try
        {
            env = JsonSerializer.Deserialize<OutboxEnvelope>(payload!, JsonOptions.Default);
            if (env is null)
            {
                reason = "InvalidEnvelope";
                return false;
            }
            return true;
        }
        catch
        {
            reason = "InvalidEnvelope";
            return false;
        }
    }

    private static bool TryDeserializeEvent(OutboxEnvelope env, out IDomainEvent? ev, out string? badType)
    {
        ev = null;
        badType = null;

        var type = ResolveType(env.EventType);
        if (type is null)
        {
            badType = env.EventType;
            return false;
        }

        try
        {
            ev = JsonSerializer.Deserialize(env.Payload, type, JsonOptions.Default) as IDomainEvent;
            return ev is not null;
        }
        catch
        {
            badType = env.EventType;
            return false;
        }
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
}
