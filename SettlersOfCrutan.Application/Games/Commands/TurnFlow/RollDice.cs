using Microsoft.Extensions.Logging;
using SettlersOfCrutan.Application;
using SettlersOfCrutan.Application.Abstractions;
using SettlersOfCrutan.Application.Abstractions.Realtime;
using SettlersOfCrutan.Application.Games;
using SettlersOfCrutan.Application.Games.DTOs;
using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.Games;

namespace SettlersOfCrutan.Application.Games.Commands.TurnFlow;

public record RollDiceCommandResult(int Dice1, int Dice2);
public record RollDiceCommand(GameId GameId) : ICommand<RollDiceCommandResult>;
public sealed class RollDiceCommandHandler(
    IGameRepository gameRepository,
    ICurrentUser currentUser,
    IUserRepository userRepository,
    IRealtimePublisher realtimePublisher,
    IDateTimeProvider clock,
    ILogger<RollDiceCommandHandler> logger) : ICommandHandler<RollDiceCommand, RollDiceCommandResult>
{
    private readonly IGameRepository _gameRepository = gameRepository;
    private readonly ICurrentUser _currentUser = currentUser;
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IRealtimePublisher _realtimePublisher = realtimePublisher;
    private readonly IDateTimeProvider _clock = clock;
    private readonly ILogger<RollDiceCommandHandler> _logger = logger;

    public async Task<Result<RollDiceCommandResult>> Handle(RollDiceCommand command, CancellationToken ct = default)
    {
        var game = await _gameRepository.GetAsync(command.GameId, ct);

        if (game is null) return Result<RollDiceCommandResult>.Failure(new Error("NotFound", "Game not found"));

        var actor = GamePlayerResolution.ResolveActor(game, await _currentUser.UserId());
        if (actor.IsFailure) return Result<RollDiceCommandResult>.Failure(actor.Error);

        var result = game.RollAndResolveProduction(actor.Value);

        if (result.IsFailure) return Result.Failure<RollDiceCommandResult>(result.Error);

        var saved = await _gameRepository.SaveAsync(game, ct);

        if (saved)
        {
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
                _logger.LogWarning(ex, "Failed to publish GameStateUpdated for GameId {GameId}", game.Id);
            }
        }

        return saved
            ? Result.Success(new RollDiceCommandResult(result.Value.Item1, result.Value.Item2))
            : Result<RollDiceCommandResult>.Failure(result.Error);
    }
}
