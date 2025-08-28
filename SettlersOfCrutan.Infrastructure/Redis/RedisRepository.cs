using Microsoft.Extensions.Options;
using SettlersOfCrutan.Infrastructure.Redis.Serialization;
using StackExchange.Redis;
using System.Text.Json;
using SettlersOfCrutan.Application.Abstractions;
using SettlersOfCrutan.Domain.Core;

namespace SettlersOfCrutan.Infrastructure.Redis;
public class RedisRepository<TAgg, TId>(IDatabase db, IOptions<RedisOptions> opts) : IRepository<TAgg, TId>
    where TAgg : AggregateRoot<TId>
    where TId : BaseId
{
    private readonly IDatabase _db = db;
    private readonly RedisOptions _opts = opts.Value ?? new RedisOptions();
    private readonly string _aggName = typeof(TAgg).Name.ToLowerInvariant();

    public async Task<TAgg?> GetAsync(TId id, CancellationToken ct = default)
    {
        var key = RedisKeys.Aggregate(_opts.KeyPrefix, _aggName, id);
        var json = await _db.StringGetAsync(key).ConfigureAwait(false);
        if (json.IsNullOrEmpty) return default;

        return JsonSerializer.Deserialize<TAgg>(json!, JsonOptions.Default);
    }

    public async Task<bool> SaveAsync(TAgg aggregate, CancellationToken ct = default)
    {
        // capture expected BEFORE bump
        var expected = aggregate.Version.ToString();

        // now bump for what we will store
        var toStore = CloneWithNextVersion(aggregate); // this increments Version
        var json = JsonSerializer.Serialize(toStore, JsonOptions.Default);

        RedisKey key = RedisKeys.Aggregate(_opts.KeyPrefix, _aggName, aggregate.Id);

        var result = (int)await _db.ScriptEvaluateAsync(
            Scripts.CompareExchangeSet,
            [key],
            [expected, json]).ConfigureAwait(false);

        if (result == 1 && _opts.DefaultTtl is { } ttl)
            await _db.KeyExpireAsync(key, ttl).ConfigureAwait(false);

        return result == 1;
    }

    public Task<bool> DeleteAsync(TId id, CancellationToken ct = default)
    {
        var key = RedisKeys.Aggregate(_opts.KeyPrefix, _aggName, id);
        return _db.KeyDeleteAsync(key);
    }

    private static TAgg CloneWithNextVersion(TAgg original)
    {
        // For now, we just bump version on the original aggregate.
        original.BumpVersion();
        return original;
    }
}