using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace SettlersOfCrutan.Infrastructure.SignalR;

public interface ICrutanClient
{
    // Clients will handle signals for this that are of type "Receive"
    Task Receive(string channel, object payload);
}

[Authorize] // cookie auth required
public class CrutanHub : Hub<ICrutanClient>
{
    // Call from joining lobby page
    public Task JoinLobby(string lobbyId) =>
        Groups.AddToGroupAsync(Context.ConnectionId, LobbyGroup(lobbyId));

    // Call when explicitly leaving lobby page
    public Task LeaveLobby(string lobbyId) =>
        Groups.RemoveFromGroupAsync(Context.ConnectionId, LobbyGroup(lobbyId));

    public Task JoinGame(string gameId) =>
        Groups.AddToGroupAsync(Context.ConnectionId, GameGroup(gameId));

    public Task LeaveGame(string gameId) =>
        Groups.RemoveFromGroupAsync(Context.ConnectionId, GameGroup(gameId));

    private static string LobbyGroup(string id) => $"crutan:lobby:{id}";
    private static string GameGroup(string id) => $"crutan:game:{id}";
}