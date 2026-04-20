using Microsoft.Extensions.Logging;
using SettlersOfCrutan.Application;
using SettlersOfCrutan.Application.Abstractions;
using SettlersOfCrutan.Application.Abstractions.Realtime;
using SettlersOfCrutan.Application.Games.DTOs;
using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.DomainErrors;
using SettlersOfCrutan.Domain.Games;
using SettlersOfCrutan.Domain.Games.Boards.Coordinates;

namespace SettlersOfCrutan.Application.Games.Commands.Build;

public record BuildInitialCommand(GameId GameId, Vertex SettlementCoordinate, Edge RoadCoordinate) : ICommand;
public sealed class BuildInitialCommandHandler(IGameRepository gameRepository,
                                                ICurrentUser currentUser,
                                                IUserRepository userRepository,
                                               IRealtimePublisher realtimePublisher,
                                               IDateTimeProvider clock,
                                               ILogger<BuildInitialCommandHandler> logger) : ICommandHandler<BuildInitialCommand>
{
    private readonly IGameRepository _gameRepository = gameRepository;
    private readonly ICurrentUser _currentUser = currentUser;
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IRealtimePublisher _realtimePublisher = realtimePublisher;
    private readonly IDateTimeProvider _clock = clock;
    private readonly ILogger<BuildInitialCommandHandler> _logger = logger;

    public async Task<Result<Nothing>> Handle(BuildInitialCommand command, CancellationToken ct = default)
    {
        var game = await _gameRepository.GetAsync(command.GameId, ct);
        if (game is null) return Result<Nothing>.Failure(DomainError.NotFound);
        var actor = GamePlayerResolution.ResolveActor(game, await _currentUser.UserId());
        if (actor.IsFailure) return Result<Nothing>.Failure(actor.Error);
        var result = game.PlaceInitial(actor.Value, command.SettlementCoordinate, command.RoadCoordinate, _clock);
        if (result.IsFailure) return Result<Nothing>.Failure(result.Error);
        await _gameRepository.SaveAsync(game, ct);

        var now = _clock.UtcNow;
        var userViews = GameDto.UserViewsFromGame(game);

        try
        {
            await _realtimePublisher.PublishGameStateToAllPlayersAsync(
                _userRepository,
                game.Id,
                userViews,
                now,
                RealtimeEvents.GameStateUpdated,
                ct);
        }
        catch (Exception ex)
        {
            // Do NOT fail the command if state is already saved.
            // Log and optionally enqueue retry/catch-up.

            _logger.LogWarning(ex, "Failed to publish GameStateUpdated for GameId {GameId}", game.Id);

            // Optional: enqueue a lightweight "game changed" signal for retry,
            // or rely on clients to resync on next poll/reconnect.
        }

        return Result<Nothing>.Success();
    }
}