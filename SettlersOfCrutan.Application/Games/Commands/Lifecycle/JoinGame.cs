using Microsoft.Extensions.Logging;
using SettlersOfCrutan.Application.Abstractions;
using SettlersOfCrutan.Application.Abstractions.Realtime;
using SettlersOfCrutan.Application.Games;
using SettlersOfCrutan.Application.Games.DTOs;
using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.DomainErrors;
using SettlersOfCrutan.Domain.Games;

namespace SettlersOfCrutan.Application.Games.Commands.Lifecycle;
public record JoinGameCommand(GameId GameId, PlayerId PlayerId) : ICommand<GameId>;
public sealed class JoinGameCommandHandler(IGameRepository gameRepository,
                                           IRealtimePublisher realtimePublisher,
                                           ILogger<JoinGameCommandHandler> logger,
                                           IDateTimeProvider clock) : ICommandHandler<JoinGameCommand, GameId>
{
    private readonly IGameRepository _gameRepository = gameRepository;
    private readonly IRealtimePublisher _realtimePublisher = realtimePublisher;
    private readonly ILogger<JoinGameCommandHandler> _logger = logger;
    private readonly IDateTimeProvider _clock = clock;

    public async Task<Result<GameId>> Handle(JoinGameCommand command, CancellationToken ct = default)
    {
        var game = await _gameRepository.GetAsync(command.GameId, ct);

        if (game is null)
            return Result<GameId>.Failure(new Error("NotFound", "Game not found"));

        var joinResult = game.JoinPlayer(command.PlayerId.Value, _clock.UtcNow);

        if (joinResult.IsFailure)
            return Result<GameId>.Failure(joinResult.Error);

        if (game.AllPlayersJoined())
            game.StartGame(_clock /*, TimeDuration */);

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

        return saved ? Result<GameId>.Success(game.Id) : Result<GameId>.Failure(new Error("Persistence", "Failed to save game state"));
    }
}
