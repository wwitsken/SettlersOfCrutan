using SettlersOfCrutan.Application.Abstractions;
using SettlersOfCrutan.Domain.PlayerPresence;

namespace SettlersOfCrutan.Infrastructure.Redis.Repositories;
public sealed class RedisPlayerPresenceRepository(RedisRepository<PlayerPresence, PlayerPresenceId> inner) : IPlayerPresenceRepository
{
    private readonly RedisRepository<PlayerPresence, PlayerPresenceId> _inner = inner;
    public Task<bool> DeleteAsync(PlayerPresenceId id, CancellationToken ct = default) => _inner.DeleteAsync(id, ct);
    public Task<PlayerPresence?> GetAsync(PlayerPresenceId id, CancellationToken ct = default) => _inner.GetAsync(id, ct);
    public Task<bool> SaveAsync(PlayerPresence aggregate, CancellationToken ct = default) => _inner.SaveAsync(aggregate, ct);
}
