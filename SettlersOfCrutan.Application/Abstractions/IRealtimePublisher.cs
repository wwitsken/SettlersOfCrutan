using SettlersOfCrutan.Domain.Games;
using SettlersOfCrutan.Domain.Lobbies;

namespace SettlersOfCrutan.Application.Abstractions;
public interface IRealtimePublisher
{
    Task ToLobbyUserAsync(LobbyId lobbyId, string userId, string eventName, object payload, CancellationToken ct = default);
    Task ToGameUserAsync(GameId gameId, string userId, string eventName, object payload, CancellationToken ct = default);
    Task ToLobbyUsersAsync(LobbyId lobbyId, IReadOnlyList<string> userIds, string eventName, object payload, CancellationToken ct = default);
    Task ToGameUsersAsync(GameId gameId, IReadOnlyList<string> userIds, string eventName, object payload, CancellationToken ct = default);
}