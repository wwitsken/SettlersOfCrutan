using Microsoft.Extensions.Logging;
using SettlersOfCrutan.Application.Abstractions;
using SettlersOfCrutan.Application.Abstractions.Realtime;
using SettlersOfCrutan.Application.Games;
using SettlersOfCrutan.Application.Games.DTOs;
using SettlersOfCrutan.Application.Games.Policies;
using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.DomainErrors;
using SettlersOfCrutan.Domain.Games;
using SettlersOfCrutan.Domain.Games.Boards.Coordinates;

namespace SettlersOfCrutan.Application.Games.Commands.Build;

public record BuildRoadCommand(GameId GameId, PlayerId PlayerId, Edge Edge) : ICommand;

public sealed class BuildRoadCommandHandler(IGameRepository gameRepository,
                                               IRealtimePublisher realtimePublisher,
                                               IDateTimeProvider clock,
                                               ILogger<BuildInitialCommandHandler> logger,
                                               StandardPriceCalculator priceCalculator) : ICommandHandler<BuildRoadCommand>
{
    private readonly IGameRepository _gameRepository = gameRepository;
    private readonly IRealtimePublisher _realtimePublisher = realtimePublisher;
    private readonly IDateTimeProvider _clock = clock;
    private readonly ILogger<BuildInitialCommandHandler> _logger = logger;
    private readonly StandardPriceCalculator _priceCalculator = priceCalculator;

    public async Task<Result<Nothing>> Handle(BuildRoadCommand command, CancellationToken ct = default)
    {
        var game = await _gameRepository.GetAsync(command.GameId, ct);
        if (game is null) return Result<Nothing>.Failure(DomainError.NotFound);

        var actor = GamePlayerResolution.ResolveActor(game, command.PlayerId);
        if (actor.IsFailure) return Result<Nothing>.Failure(actor.Error);
        var result = game.BuildRoad(_priceCalculator, actor.Value, command.Edge);
        if (result.IsFailure) return Result<Nothing>.Failure(result.Error);

        var saved = await _gameRepository.SaveAsync(game, ct);

        var now = _clock.UtcNow;
        var userViews = GameDto.UserViewsFromGame(game);

        try
        {
            var publishTasks = userViews.Select(kvp =>
                _realtimePublisher.UpdateGameAsync(
                    game.Id,
                    kvp.Key,                 // user id / connection routing key
                    now,
                    RealtimeEvents.GameStateUpdated,
                    kvp.Value,
                    ct));

            await Task.WhenAll(publishTasks);
        }
        catch (Exception ex)
        {
            // Do NOT fail the command if state is already saved.
            // Log and optionally enqueue retry/catch-up.

            _logger.LogWarning(ex, "Failed to publish GameStateUpdated for GameId {GameId}", game.Id);

            // Optional: enqueue a lightweight "game changed" signal for retry,
            // or rely on clients to resync on next poll/reconnect.
        }

        return saved ? Result<Nothing>.Success() : Result<Nothing>.Failure(DomainError.InvalidOperation);
    }
}
