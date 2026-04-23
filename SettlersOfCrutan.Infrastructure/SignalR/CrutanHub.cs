using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using SettlersOfCrutan.Application.Abstractions;
using SettlersOfCrutan.Application.Games.DTOs;
using SettlersOfCrutan.Application.Games.Queries;
using SettlersOfCrutan.Domain.Games;

namespace SettlersOfCrutan.Infrastructure.SignalR;

public interface ICrutanClient
{
    // Clients will handle signals for this that are of type "Receive"
    Task GameReceive(string gameId, DateTimeOffset timeStamp, string eventName, object payload);
    Task LobbyReceive(string lobbyId, DateTimeOffset timeStamp, string eventName, object payload);
    Task MoveFromLobbyToGame(string lobbyId, string gameId, DateTimeOffset timeStamp);
}

//[Authorize] // cookie auth required
public class CrutanHub(
    IQueryHandler<GetUsersInGameQuery, PrincipalIdsInGameDto> usersInGameHandler,
    IUserRepository userRepository) : Hub<ICrutanClient>
{
    /// <summary>
    /// Hub method invoked by clients to broadcast a chat message to every user in a game.
    /// Enriches the payload with the sender's surrogate <c>UserId</c> so clients can resolve
    /// display name / color from their already-loaded player roster.
    /// </summary>
    public async Task SendGameMessage(Guid gameId, string message)
    {
        if (string.IsNullOrWhiteSpace(message)) return;

        var principalId = Context.UserIdentifier;
        if (string.IsNullOrEmpty(principalId)) return;

        var sender = await userRepository.GetByPrincipalIdAsync(principalId);
        if (sender is null) return;

        var usersInGame = await usersInGameHandler.Handle(new(new() { Value = gameId }));
        if (!usersInGame.IsSuccess) return;

        var principalIds = usersInGame.Value.PrincipalIds.ToArray();

        // Only participants of the game are allowed to chat in it.
        if (!principalIds.Contains(principalId)) return;

        // Trim + cap length defensively; keeps UI from having to deal with absurd payloads.
        var trimmed = message.Trim();
        if (trimmed.Length > 500) trimmed = trimmed[..500];

        var payload = new GameChatMessageDto(sender.Id.Value, trimmed);

        await Clients.Users(principalIds)
            .GameReceive(gameId.ToString(), DateTimeOffset.UtcNow, "gameMessage", payload);
    }
}
