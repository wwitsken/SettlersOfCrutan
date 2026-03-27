using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace SettlersOfCrutan.Infrastructure.SignalR;

public interface ICrutanClient
{
    // Clients will handle signals for this that are of type "Receive"
    Task GameReceive(string gameId, DateTimeOffset timeStamp, string eventName, object payload);
    Task LobbyReceive(string lobbyId, DateTimeOffset timeStamp, string eventName, object payload);
    Task MoveFromLobbyToGame(string lobbyId, string gameId, DateTimeOffset timeStamp);
}

//[Authorize] // cookie auth required
public class CrutanHub : Hub<ICrutanClient>
{ }