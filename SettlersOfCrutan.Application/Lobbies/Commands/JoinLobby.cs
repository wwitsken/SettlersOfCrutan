
using Microsoft.Extensions.Logging;
using SettlersOfCrutan.Application.Abstractions;
using SettlersOfCrutan.Application.Abstractions.Realtime;
using SettlersOfCrutan.Application.Lobbies.DTOs;
using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.DomainErrors;
using SettlersOfCrutan.Domain.Games;
using SettlersOfCrutan.Domain.Lobbies;

namespace SettlersOfCrutan.Application.Lobbies.Commands;
public record JoinLobbyCommand(Guid LobbyId, PlayerId PlayerId) : ICommand;
public sealed class JoinLobbyCommandHandler(ILobbyRepository lobbyRepository,
                                            IRealtimePublisher realtimePublisher,
                                            IDateTimeProvider clock,
                                            ILogger<JoinLobbyCommandHandler> logger) : ICommandHandler<JoinLobbyCommand>
{
    private readonly ILobbyRepository _lobbyRepository = lobbyRepository;
    private readonly IRealtimePublisher _realtimePublisher = realtimePublisher;
    private readonly IDateTimeProvider _clock = clock;
    private readonly ILogger<JoinLobbyCommandHandler> _logger = logger;

    public async Task<Result<Nothing>> Handle(JoinLobbyCommand command, CancellationToken ct = default)
    {
        var lobby = await _lobbyRepository.GetAsync(new LobbyId() { Value = command.LobbyId }, ct);
        if (lobby is null) return Result<Nothing>.Failure(DomainError.NotFound);

        var res = lobby.AddMember(command.PlayerId);

        if (res.IsFailure) return Result<Nothing>.Failure(res.Error);
        await _lobbyRepository.SaveAsync(lobby, ct);

        var now = _clock.UtcNow;
        var userViews = LobbyDto.UserViewsFromLobby(lobby);

        try
        {
            var publishTasks = userViews.Select(kvp =>
                _realtimePublisher.UpdateLobbyAsync(
                    lobby.Id,
                    kvp.Key,                 // user id / connection routing key
                    now,
                    RealtimeEvents.UserJoinedLobby,
                    kvp.Value,
                    ct));

            await Task.WhenAll(publishTasks);
        }
        catch (Exception ex)
        {
            // Do NOT fail the command if state is already saved.
            // Log and optionally enqueue retry/catch-up.

            _logger.LogWarning(ex, "Failed to publish UserJoinedLobby for LobbyId {LobbyId}", lobby.Id);

            // Optional: enqueue a lightweight "lobby changed" signal for retry,
            // or rely on clients to resync on next poll/reconnect.
        }

        return Result.Success();
    }
}
