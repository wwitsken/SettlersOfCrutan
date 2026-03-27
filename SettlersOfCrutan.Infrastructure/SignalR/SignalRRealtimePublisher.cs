using Microsoft.AspNetCore.SignalR;
using SettlersOfCrutan.Application.Abstractions;
using SettlersOfCrutan.Domain.Games;
using SettlersOfCrutan.Domain.Lobbies;

namespace SettlersOfCrutan.Infrastructure.SignalR;
public sealed class SignalRRealtimePublisher(IHubContext<CrutanHub, ICrutanClient> hub)
        : IRealtimePublisher
{
    private readonly IHubContext<CrutanHub, ICrutanClient> _hub = hub;

    public Task UpdateLobbyAsync(LobbyId lobbyId, string userId, DateTimeOffset timestamp, string eventName, object payload, CancellationToken ct = default)
    {
        return _hub.Clients.User(userId).LobbyReceive(lobbyId.Value.ToString(), timestamp, eventName, payload);
    }

    public Task UpdateLobbyAsync(LobbyId lobbyId, IReadOnlyList<string> userIds, DateTimeOffset timestamp, string eventName, object payload, CancellationToken ct = default)
    {
        return _hub.Clients.Users(userIds).LobbyReceive(lobbyId.Value.ToString(), timestamp, eventName, payload);
    }

    public Task UpdateGameAsync(GameId gameId, string userId, DateTimeOffset timestamp, string eventName, object payload, CancellationToken ct = default)
    {
        return _hub.Clients.User(userId).GameReceive(gameId.Value.ToString(), timestamp, eventName, payload);
    }

    public Task UpdateGameAsync(GameId gameId, IReadOnlyList<string> userIds, DateTimeOffset timestamp, string eventName, object payload, CancellationToken ct = default)
    {
        return _hub.Clients.Users(userIds).GameReceive(gameId.Value.ToString(), timestamp, eventName, payload);
    }

    public Task MoveFromLobbyToGameAsync(LobbyId lobbyId, GameId gameId, IReadOnlyList<string> userIds, DateTimeOffset timestamp, CancellationToken ct = default)
    {
        return _hub.Clients.Users(userIds).MoveFromLobbyToGame(lobbyId.Value.ToString(), gameId.Value.ToString(), timestamp);
    }
}