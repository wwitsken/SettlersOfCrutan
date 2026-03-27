using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SettlersOfCrutan.Application.Abstractions;
using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Infrastructure.Outbox;
using SettlersOfCrutan.Infrastructure.Redis.Serialization;
using StackExchange.Redis;
using System.Text.Json;

namespace SettlersOfCrutan.Infrastructure.Redis;
public class RedisRepository<TAgg, TId>(
    IDatabase db,
    IOptions<RedisOptions> opts,
    IDomainEventPublisher domainEventPublisher,
    ILogger<RedisRepository<TAgg, TId>> logger) : IRepository<TAgg, TId>
    where TAgg : AggregateRoot<TId>
    where TId : BaseId
{
    private readonly IDatabase _db = db;
    private readonly RedisOptions _opts = opts.Value ?? new RedisOptions();
    private readonly IDomainEventPublisher _domainEventPublisher = domainEventPublisher;
    private readonly ILogger<RedisRepository<TAgg, TId>> _logger = logger;
    private readonly string _aggName = typeof(TAgg).Name.ToLowerInvariant();

    public async Task<TAgg?> GetAsync(TId id, CancellationToken ct = default)
    {
        var key = RedisKeys.Aggregate(_opts.KeyPrefix, _aggName, id);
        var json = await _db.StringGetAsync(key);
        if (json.IsNullOrEmpty) return default;

        return JsonSerializer.Deserialize<TAgg>(json.ToString(), JsonOptions.Default);
    }

    public async Task<bool> SaveAsync(TAgg aggregate, CancellationToken ct = default)
    {
        var expected = aggregate.Version.ToString();
        var toStore = CloneWithNextVersion(aggregate);
        var json = JsonSerializer.Serialize(toStore, JsonOptions.Default);

        RedisKey key = RedisKeys.Aggregate(_opts.KeyPrefix, _aggName, aggregate.Id);

        List<RedisValue> argv = [expected, json, "0"];

        var result = (int)await _db.ScriptEvaluateAsync(
            Scripts.CompareExchangeSet,
            [key],
            [.. argv]);

        if (result == 1 && _opts.DefaultTtl is { } ttl)
            await _db.KeyExpireAsync(key, ttl);

        if (result == 1)
        {
            var pendingEvents = aggregate.DomainEvents.ToList();
            aggregate.ClearRaisedDomainEvents();

            foreach (var ev in pendingEvents)
            {
                try
                {
                    await _domainEventPublisher.PublishAsync(ev, ct);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex,
                        "Failed to publish domain event {EventType} for aggregate {AggName} {AggId}",
                        ev.GetType().Name, _aggName, aggregate.Id);
                }
            }
        }

        return result == 1;
    }

    public Task<bool> DeleteAsync(TId id, CancellationToken ct = default)
    {
        var key = RedisKeys.Aggregate(_opts.KeyPrefix, _aggName, id);
        return _db.KeyDeleteAsync(key);
    }

    private static TAgg CloneWithNextVersion(TAgg original)
    {
        original.BumpVersion();
        return original;
    }
}
