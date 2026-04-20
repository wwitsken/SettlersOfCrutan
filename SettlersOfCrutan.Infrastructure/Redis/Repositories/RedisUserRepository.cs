using Microsoft.Extensions.Options;
using SettlersOfCrutan.Application.Abstractions;
using SettlersOfCrutan.Domain.Users;
using StackExchange.Redis;

namespace SettlersOfCrutan.Infrastructure.Redis.Repositories;

public sealed class RedisUserRepository(
    RedisRepository<User, UserId> inner,
    IDatabase db,
    IOptions<RedisOptions> opts) : IUserRepository
{
    private readonly RedisRepository<User, UserId> _inner = inner;
    private readonly IDatabase _db = db;
    private readonly RedisOptions _opts = opts.Value ?? new RedisOptions();

    public Task<User?> GetAsync(UserId id, CancellationToken ct = default) => _inner.GetAsync(id, ct);

    public Task<IReadOnlyList<User>> GetManyAsync(IEnumerable<UserId> ids, CancellationToken ct = default) => _inner.GetManyAsync(ids, ct);

    public async Task<User?> GetByPrincipalIdAsync(string principalId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(principalId)) return null;
        var key = PrincipalIndexKey(principalId);
        var raw = await _db.StringGetAsync(key);
        if (raw.IsNullOrEmpty) return null;
        if (!Guid.TryParse(raw.ToString(), out var guid)) return null;
        return await _inner.GetAsync(UserId.Create(guid), ct);
    }

    public async Task<bool> SaveAsync(User aggregate, CancellationToken ct = default)
    {
        var previous = await _inner.GetAsync(aggregate.Id, ct);
        var ok = await _inner.SaveAsync(aggregate, ct);
        if (!ok || string.IsNullOrWhiteSpace(aggregate.PrincipalId))
            return ok;

        if (previous is not null
            && !string.IsNullOrWhiteSpace(previous.PrincipalId)
            && !string.Equals(previous.PrincipalId, aggregate.PrincipalId, StringComparison.Ordinal))
        {
            await _db.KeyDeleteAsync(PrincipalIndexKey(previous.PrincipalId));
        }

        await _db.StringSetAsync(PrincipalIndexKey(aggregate.PrincipalId), aggregate.Id.Value.ToString());
        return true;
    }

    public async Task<bool> DeleteAsync(UserId id, CancellationToken ct = default)
    {
        var existing = await _inner.GetAsync(id, ct);
        var ok = await _inner.DeleteAsync(id, ct);
        if (ok && existing is not null && !string.IsNullOrWhiteSpace(existing.PrincipalId))
            await _db.KeyDeleteAsync(PrincipalIndexKey(existing.PrincipalId));
        return ok;
    }

    private RedisKey PrincipalIndexKey(string principalId) =>
        $"{_opts.KeyPrefix}:user:principal:{principalId}";
}
