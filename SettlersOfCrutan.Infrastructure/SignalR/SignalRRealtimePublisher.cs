using Microsoft.AspNetCore.SignalR;
using SettlersOfCrutan.Application.Abstractions;
using SettlersOfCrutan.Domain.Games;
using SettlersOfCrutan.Domain.Lobbies;
using SettlersOfCrutan.Domain.Users;

namespace SettlersOfCrutan.Infrastructure.SignalR;

public sealed class SignalRRealtimePublisher(IHubContext<CrutanHub, ICrutanClient> hub)
    : IRealtimePublisher
{
    private readonly IHubContext<CrutanHub, ICrutanClient> _hub = hub;

    public Task UpdateLobbyAsync(LobbyId lobbyId, User user, DateTimeOffset timestamp, string eventName, object payload, CancellationToken ct = default)
    {
        string key = user.PrincipalId;
        return _hub.Clients.User(key).LobbyReceive(lobbyId.Value.ToString(), timestamp, eventName, payload);
    }

    public Task UpdateLobbyAsync(LobbyId lobbyId, IReadOnlyList<User> users, DateTimeOffset timestamp, string eventName, object payload, CancellationToken ct = default)
    {
        var keys = users.Select(u => u.PrincipalId).ToArray();
        return _hub.Clients.Users(keys).LobbyReceive(lobbyId.Value.ToString(), timestamp, eventName, payload);
    }

    public Task UpdateGameAsync(GameId gameId, User user, DateTimeOffset timestamp, string eventName, object payload, CancellationToken ct = default)
    {
        string key = user.PrincipalId;

        return _hub.Clients.User(key).GameReceive(gameId.Value.ToString(), timestamp, eventName, payload);
    }

    public Task UpdateGameAsync(GameId gameId, IReadOnlyList<User> users, DateTimeOffset timestamp, string eventName, object payload, CancellationToken ct = default)
    {
        var keys = users.Select(u => u.PrincipalId).ToArray();
        return _hub.Clients.Users(keys).GameReceive(gameId.Value.ToString(), timestamp, eventName, payload);
    }

    public Task MoveFromLobbyToGameAsync(LobbyId lobbyId, GameId gameId, IReadOnlyList<User> users, DateTimeOffset timestamp, CancellationToken ct = default)
    {
        var keys = users.Select(u => u.PrincipalId).ToArray();

        return _hub.Clients.Users(keys).MoveFromLobbyToGame(lobbyId.Value.ToString(), gameId.Value.ToString(), timestamp);
    }
}
