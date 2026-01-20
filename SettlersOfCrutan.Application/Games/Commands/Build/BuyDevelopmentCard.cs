using Microsoft.Extensions.Logging;
using SettlersOfCrutan.Application.Abstractions;
using SettlersOfCrutan.Application.Abstractions.Realtime;
using SettlersOfCrutan.Application.Games.DTOs;
using SettlersOfCrutan.Application.Games.Policies;
using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.DomainErrors;
using SettlersOfCrutan.Domain.Games;
using SettlersOfCrutan.Domain.Games.Resources;

namespace SettlersOfCrutan.Application.Games.Commands.Build;

public record BuyDevelopmentCardCommand(GameId GameId, PlayerId PlayerId) : ICommand<DevelopmentCardType>;

public sealed class BuyDevelopmentCardCommandHandler(
    IGameRepository gameRepository,
    IRealtimePublisher realtimePublisher,
    IDateTimeProvider clock,
    ILogger<BuyDevelopmentCardCommandHandler> logger,
    StandardPriceCalculator priceCalculator) : ICommandHandler<BuyDevelopmentCardCommand, DevelopmentCardType>
{
    private readonly IGameRepository _gameRepository = gameRepository;
    private readonly IRealtimePublisher _realtimePublisher = realtimePublisher;
    private readonly IDateTimeProvider _clock = clock;
    private readonly ILogger<BuyDevelopmentCardCommandHandler> _logger = logger;
    private readonly StandardPriceCalculator _priceCalculator = priceCalculator;

    public async Task<Result<DevelopmentCardType>> Handle(BuyDevelopmentCardCommand command, CancellationToken ct = default)
    {
        var game = await _gameRepository.GetAsync(command.GameId, ct);
        if (game is null) return Result<DevelopmentCardType>.Failure(DomainError.NotFound);

        var result = game.BuyDevelopmentCard(_priceCalculator, command.PlayerId);
        if (result.IsFailure) return Result<DevelopmentCardType>.Failure(result.Error);

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

        return saved
            ? Result<DevelopmentCardType>.Success(result.Value)
            : Result<DevelopmentCardType>.Failure(DomainError.InvalidOperation);
    }
}
