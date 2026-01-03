using SettlersOfCrutan.Domain.Games;
using SettlersOfCrutan.Domain.Lobbies;

namespace SettlersOfCrutan.Application.Abstractions;
public interface IRealtimePublisher
{
    Task UpdateLobbyAsync(LobbyId lobbyId, string userId, DateTimeOffset timestamp, string eventName, object payload, CancellationToken ct = default);
    Task UpdateLobbyAsync(LobbyId lobbyId, IReadOnlyList<string> userIds, DateTimeOffset timestamp, string eventName, object payload, CancellationToken ct = default);
    Task MoveFromLobbyToGameAsync(LobbyId lobbyId, GameId gameId, IReadOnlyList<string> userIds, DateTimeOffset timestamp, CancellationToken ct = default);
    Task UpdateGameAsync(GameId gameId, string userId, DateTimeOffset timestamp, string eventName, object payload, CancellationToken ct = default);
    Task UpdateGameAsync(GameId gameId, IReadOnlyList<string> userIds, DateTimeOffset timestamp, string eventName, object payload, CancellationToken ct = default);

}