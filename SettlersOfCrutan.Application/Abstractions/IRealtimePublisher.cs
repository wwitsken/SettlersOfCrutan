using SettlersOfCrutan.Domain.Games;
using SettlersOfCrutan.Domain.Lobbies;
using SettlersOfCrutan.Domain.Users;

namespace SettlersOfCrutan.Application.Abstractions;
public interface IRealtimePublisher
{
    Task UpdateLobbyAsync(LobbyId lobbyId, User user, DateTimeOffset timestamp, string eventName, object payload, CancellationToken ct = default);
    Task UpdateLobbyAsync(LobbyId lobbyId, IReadOnlyList<User> users, DateTimeOffset timestamp, string eventName, object payload, CancellationToken ct = default);
    Task MoveFromLobbyToGameAsync(LobbyId lobbyId, GameId gameId, IReadOnlyList<User> users, DateTimeOffset timestamp, CancellationToken ct = default);
    Task UpdateGameAsync(GameId gameId, User user, DateTimeOffset timestamp, string eventName, object payload, CancellationToken ct = default);
    Task UpdateGameAsync(GameId gameId, IReadOnlyList<User> users, DateTimeOffset timestamp, string eventName, object payload, CancellationToken ct = default);

}