namespace SettlersOfCrutan.Application.Games.DTOs;

/// <summary>
/// Payload broadcast over SignalR for a chat message sent within a game.
/// <see cref="SenderUserId"/> is the Domain <c>UserId</c> (surrogate) so clients
/// can resolve display name / color from their already-loaded player roster.
/// </summary>
public record GameChatMessageDto(Guid SenderUserId, string Message);
