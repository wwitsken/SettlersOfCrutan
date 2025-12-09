using Microsoft.AspNetCore.SignalR;
using SettlersOfCrutan.Application.Abstractions;
using SettlersOfCrutan.Domain.Games;
using SettlersOfCrutan.Domain.Lobbies;

namespace SettlersOfCrutan.Infrastructure.SignalR;
public sealed class SignalRRealtimePublisher(IHubContext<CrutanHub, ICrutanClient> hub)
        : IRealtimePublisher
{
    private readonly IHubContext<CrutanHub, ICrutanClient> _hub = hub;

    public Task ToLobbyUserAsync(LobbyId lobbyId, string userId, string eventName, object payload, CancellationToken ct = default) =>
        _hub.Clients.User(userId).LobbyReceive(lobbyId.Value.ToString(), eventName, payload);
    
    public Task ToLobbyUsersAsync(LobbyId lobbyId, IReadOnlyList<string> userIds, string eventName, object payload, CancellationToken ct = default) =>
        _hub.Clients.Users(userIds).LobbyReceive(lobbyId.Value.ToString(), eventName, payload);

    public Task ToGameUserAsync(GameId gameId, string userId, string eventName, object payload, CancellationToken ct = default) =>
        _hub.Clients.User(userId).GameReceive(gameId.Value.ToString(), eventName, payload);

    public Task ToGameUsersAsync(GameId gameId, IReadOnlyList<string> userIds, string eventName, object payload, CancellationToken ct = default) =>
        _hub.Clients.Users(userIds).GameReceive(gameId.Value.ToString(), eventName, payload);
}