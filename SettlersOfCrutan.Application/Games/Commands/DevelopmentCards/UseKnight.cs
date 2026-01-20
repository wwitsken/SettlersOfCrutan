using Microsoft.Extensions.Logging;
using SettlersOfCrutan.Application.Abstractions;
using SettlersOfCrutan.Application.Abstractions.Realtime;
using SettlersOfCrutan.Application.Games.DTOs;
using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.DomainErrors;
using SettlersOfCrutan.Domain.Games;
using SettlersOfCrutan.Domain.Games.Boards.Coordinates;

namespace SettlersOfCrutan.Application.Games.Commands.DevelopmentCards;

public record UseKnightCommand(GameId GameId, PlayerId PlayerId, HexCoord NewRobberHexCoord, PlayerId VictimId) : ICommand;

public sealed class UseKnightCommandHandler(
    IGameRepository gameRepository,
    IRealtimePublisher realtimePublisher,
    IDateTimeProvider clock,
    ILogger<UseKnightCommandHandler> logger) : ICommandHandler<UseKnightCommand>
{
    private readonly IGameRepository _gameRepository = gameRepository;
    private readonly IRealtimePublisher _realtimePublisher = realtimePublisher;
    private readonly IDateTimeProvider _clock = clock;
    private readonly ILogger<UseKnightCommandHandler> _logger = logger;

    public async Task<Result<Nothing>> Handle(UseKnightCommand command, CancellationToken ct = default)
    {
        var game = await _gameRepository.GetAsync(command.GameId, ct);
        if (game is null) return Result.Failure(DomainError.NotFound);

        var result = game.PlayKnight(command.PlayerId, command.NewRobberHexCoord, command.VictimId);
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
