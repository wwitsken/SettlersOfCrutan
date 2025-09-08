using Microsoft.AspNetCore.SignalR;

namespace SettlersOfCrutan.Infrastructure.SignalR;

public class GameHub : Hub
{
    public Task JoinGameGroup(string gameId) =>
        Groups.AddToGroupAsync(Context.ConnectionId, GroupName(gameId));

    public Task LeaveGameGroup(string gameId) =>
        Groups.RemoveFromGroupAsync(Context.ConnectionId, GroupName(gameId));

    private static string GroupName(string gameId) => $"game:{gameId}";
}
