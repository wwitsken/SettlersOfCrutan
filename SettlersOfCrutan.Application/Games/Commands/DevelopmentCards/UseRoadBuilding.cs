using Microsoft.Extensions.Logging;
using SettlersOfCrutan.Application.Abstractions;
using SettlersOfCrutan.Application.Abstractions.Realtime;
using SettlersOfCrutan.Application.Games.DTOs;
using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.DomainErrors;
using SettlersOfCrutan.Domain.Games;
using SettlersOfCrutan.Domain.Games.Boards;
using SettlersOfCrutan.Domain.Games.Boards.Coordinates;

namespace SettlersOfCrutan.Application.Games.Commands.DevelopmentCards;

public record UseRoadBuildingCommand(GameId GameId, PlayerId PlayerId, Edge Edge1, Edge Edge2) : ICommand;

public sealed class UseRoadBuildingCommandHandler(
    IGameRepository gameRepository,
    IRealtimePublisher realtimePublisher,
    IDateTimeProvider clock,
    ILogger<UseRoadBuildingCommandHandler> logger) : ICommandHandler<UseRoadBuildingCommand>
{
    private readonly IGameRepository _gameRepository = gameRepository;
    private readonly IRealtimePublisher _realtimePublisher = realtimePublisher;
    private readonly IDateTimeProvider _clock = clock;
    private readonly ILogger<UseRoadBuildingCommandHandler> _logger = logger;

    public async Task<Result<Nothing>> Handle(UseRoadBuildingCommand command, CancellationToken ct = default)
    {
        var game = await _gameRepository.GetAsync(command.GameId, ct);
        if (game is null) return Result.Failure(DomainError.NotFound);

        Result<(Road r1, Road r2)> result = game.PlayRoadBuilding(command.PlayerId, command.Edge1, command.Edge2);
        if (result.IsFailure) return Result.Failure(result.Error);

        var saved = await _gameRepository.SaveAsync(game, ct);

        if (saved)
        {
            var now = _clock.UtcNow;
            var userViews = GameDto.UserViewsFromGame(game);

            try
            {
                var publishTasks = userViews.Select(kvp =>
                    _realtimePublisher.UpdateGameAsync(
                        game.Id,
                        kvp.Key,
                        now,
                        RealtimeEvents.GameStateUpdated,
                        kvp.Value,
                        ct));

                await Task.WhenAll(publishTasks);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to publish GameStateUpdated for GameId {GameId}", game.Id);
            }
        }

        return saved ? Result.Success() : Result.Failure(DomainError.InvalidOperation);
    }
}
