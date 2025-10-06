using SettlersOfCrutan.Application.Abstractions;
using SettlersOfCrutan.Domain.Lobbies;

namespace SettlersOfCrutan.Infrastructure.Redis.Repositories;

public class RedisLobbyRepository(RedisRepository<Lobby, LobbyId> inner) : ILobbyRepository
{
    private readonly RedisRepository<Lobby, LobbyId> _inner = inner;
    public Task<Lobby?> GetAsync(LobbyId id, CancellationToken ct = default) => _inner.GetAsync(id, ct);
    public Task<bool> SaveAsync(Lobby aggregate, CancellationToken ct = default) => _inner.SaveAsync(aggregate, ct);
    public Task<bool> DeleteAsync(LobbyId id, CancellationToken ct = default) => _inner.DeleteAsync(id, ct);
}
