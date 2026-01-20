using Microsoft.Extensions.Logging;
using SettlersOfCrutan.Application.Abstractions;
using SettlersOfCrutan.Application.Abstractions.Realtime;
using SettlersOfCrutan.Application.Games.DTOs;
using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.DomainErrors;
using SettlersOfCrutan.Domain.Games;

namespace SettlersOfCrutan.Application.Games.Commands.TurnFlow;

public record EndTurnCommand(GameId GameId, PlayerId PlayerId) : ICommand<PlayerId>;

public sealed class EndTurnCommandHandler(
    IGameRepository gameRepository,
    IRealtimePublisher realtimePublisher,
    IDateTimeProvider clock,
    ILogger<EndTurnCommandHandler> logger) : ICommandHandler<EndTurnCommand, PlayerId>
{
    private readonly IGameRepository _gameRepository = gameRepository;
    private readonly IRealtimePublisher _realtimePublisher = realtimePublisher;
    private readonly IDateTimeProvider _clock = clock;
    private readonly ILogger<EndTurnCommandHandler> _logger = logger;

    public async Task<Result<PlayerId>> Handle(EndTurnCommand command, CancellationToken ct = default)
    {
        var game = await _gameRepository.GetAsync(command.GameId, ct);
        if (game is null) return Result<PlayerId>.Failure(DomainError.NotFound);

        var result = game.EndTurn(command.PlayerId, _clock);
        if (result.IsFailure) return Result<PlayerId>.Failure(result.Error);

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

        return saved ? Result<PlayerId>.Success(result.Value) : Result<PlayerId>.Failure(DomainError.InvalidOperation);
    }
}
