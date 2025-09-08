using Microsoft.AspNetCore.SignalR;
using SettlersOfCrutan.Application.Abstractions;

namespace SettlersOfCrutan.Infrastructure.SignalR;
public sealed class SignalRRealtimePublisher<THub>(IHubContext<THub> hub) : IRealtimePublisher where THub : Hub
{
    private readonly IHubContext<THub> _hub = hub;

    public Task PublishAsync<T>(string channel, T message, CancellationToken ct = default) =>
        _hub.Clients.All.SendAsync(channel, message, ct);

    public Task PublishToUserAsync<T>(string userId, T message, CancellationToken ct = default) =>
        _hub.Clients.User(userId).SendAsync(typeof(T).Name, message, ct);

    public Task PublishToGroupAsync<T>(string group, T message, CancellationToken ct = default) =>
        _hub.Clients.Group(group).SendAsync(typeof(T).Name, message, ct);
}
