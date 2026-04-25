using Microsoft.Extensions.Logging;
using SettlersOfCrutan.Application.Abstractions;
using SettlersOfCrutan.Application.Abstractions.Realtime;
using SettlersOfCrutan.Application.Lobbies.DTOs;
using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.Games;
using SettlersOfCrutan.Domain.Lobbies;
using SettlersOfCrutan.Domain.Users;

namespace SettlersOfCrutan.Application.Lobbies.Commands;

public record LeaveLobbyCommand(LobbyId LobbyId) : ICommand;
public sealed class LeaveLobbyCommandHandler(
    ILobbyRepository lobbyRepository,
    ICurrentUser currentUser,
    IUserRepository userRepository,
    IRealtimePublisher realtimePublisher,
    ILogger<LeaveLobbyCommandHandler> logger,
    IDateTimeProvider clock) : ICommandHandler<LeaveLobbyCommand>
{
    private readonly ILobbyRepository _lobbyRepository = lobbyRepository;
    private readonly ICurrentUser _currentUser = currentUser;
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IRealtimePublisher _realtimePublisher = realtimePublisher;
    private readonly IDateTimeProvider _clock = clock;
    private readonly ILogger<LeaveLobbyCommandHandler> _logger = logger;

    public async Task<Result<Nothing>> Handle(LeaveLobbyCommand command, CancellationToken ct = default)
    {
        var lobby = await _lobbyRepository.GetAsync(command.LobbyId, ct);

        if (lobby is null) return Result<Nothing>.Failure(DomainError.NotFound);

        var res = lobby.RemoveMember(await _currentUser.UserId());

        if (res.IsFailure) return res;

        await _lobbyRepository.SaveAsync(lobby, ct);

        var now = _clock.UtcNow;
        var userViews = LobbyDto.UserViewsFromLobby(lobby);

        try
        {
            await _realtimePublisher.PublishLobbyStateToAllMembersAsync(
                _userRepository,
                lobby.Id,
                userViews,
                now,
                RealtimeEvents.UserLeftLobby,
                ct);
        }
        catch (Exception ex)
        {
            // Do NOT fail the command if state is already saved.
            // Log and optionally enqueue retry/catch-up.

            _logger.LogWarning(ex, "Failed to publish UserLeftLobby for LobbyId {LobbyId}", lobby.Id);

            // Optional: enqueue a lightweight "lobby changed" signal for retry,
            // or rely on clients to resync on next poll/reconnect.
        }

        return res;
    }
}