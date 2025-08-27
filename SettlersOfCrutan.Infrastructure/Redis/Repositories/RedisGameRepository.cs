using SettlersOfCrutan.Application.Games;
using SettlersOfCrutan.Domain.Games;

namespace SettlersOfCrutan.Infrastructure.Redis.Repositories;
public class RedisGameRepository(RedisRepository<Game, GameId> inner) : IGameRepository
{
    private readonly RedisRepository<Game, GameId> _inner = inner;

    public Task<Game?> GetAsync(GameId id, CancellationToken ct = default) => _inner.GetAsync(id, ct);
    public Task<bool> SaveAsync(Game aggregate, CancellationToken ct = default) => _inner.SaveAsync(aggregate, ct);
    public Task<bool> DeleteAsync(GameId id, CancellationToken ct = default) => _inner.DeleteAsync(id, ct);
}
