using Microsoft.AspNetCore.SignalR;
using SettlersOfCrutan.Application.Abstractions;
using SettlersOfCrutan.Domain.Games;
using SettlersOfCrutan.Domain.Lobbies;

namespace SettlersOfCrutan.Infrastructure.SignalR;
public sealed class SignalRRealtimePublisher(IHubContext<CrutanHub, ICrutanClient> hub)
        : IRealtimePublisher
{
    private readonly IHubContext<CrutanHub, ICrutanClient> _hub = hub;

    public Task ToLobbyAsync(LobbyId lobbyId, string channel, object payload, CancellationToken ct = default) =>
        _hub.Clients.Group($"crutan:lobby:{lobbyId.Value}").Receive(channel, payload);

    public Task ToGameAsync(GameId gameId, string channel, object payload, CancellationToken ct = default) =>
        _hub.Clients.Group($"crutan:game:{gameId.Value}").Receive(channel, payload);

    public Task ToUserAsync(string userId, string channel, object payload, CancellationToken ct = default) =>
        _hub.Clients.User(userId).Receive(channel, payload);
}