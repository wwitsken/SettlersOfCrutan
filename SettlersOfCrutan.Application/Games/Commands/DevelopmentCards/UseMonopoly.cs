using Microsoft.Extensions.Logging;
using SettlersOfCrutan.Application.Abstractions;
using SettlersOfCrutan.Application.Abstractions.Realtime;
using SettlersOfCrutan.Application.Games.DTOs;
using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.DomainErrors;
using SettlersOfCrutan.Domain.Games;
using SettlersOfCrutan.Domain.Games.Resources;

namespace SettlersOfCrutan.Application.Games.Commands.DevelopmentCards;

public record UseMonopolyCommand(GameId GameId, PlayerId PlayerId, ResourceCardType ResourceType) : ICommand<int>;

public sealed class UseMonopolyCommandHandler(
    IGameRepository gameRepository,
    IRealtimePublisher realtimePublisher,
    IDateTimeProvider clock,
    ILogger<UseMonopolyCommandHandler> logger) : ICommandHandler<UseMonopolyCommand, int>
{
    private readonly IGameRepository _gameRepository = gameRepository;
    private readonly IRealtimePublisher _realtimePublisher = realtimePublisher;
    private readonly IDateTimeProvider _clock = clock;
    private readonly ILogger<UseMonopolyCommandHandler> _logger = logger;

    public async Task<Result<int>> Handle(UseMonopolyCommand command, CancellationToken ct = default)
    {
        var game = await _gameRepository.GetAsync(command.GameId, ct);
        if (game is null) return Result<int>.Failure(DomainError.NotFound);

        var result = game.PlayMonopoly(command.PlayerId, command.ResourceType);
        if (result.IsFailure) return Result<int>.Failure(result.Error);

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

        return saved ? Result<int>.Success(result.Value) : Result<int>.Failure(DomainError.InvalidOperation);
    }
}
