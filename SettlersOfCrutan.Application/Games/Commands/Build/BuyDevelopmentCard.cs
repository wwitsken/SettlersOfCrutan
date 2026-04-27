using Microsoft.Extensions.Logging;
using SettlersOfCrutan.Application;
using SettlersOfCrutan.Application.Abstractions;
using SettlersOfCrutan.Application.Abstractions.Realtime;
using SettlersOfCrutan.Application.Games;
using SettlersOfCrutan.Application.Games.DTOs;
using SettlersOfCrutan.Application.Games.Policies;
using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.Games;
using SettlersOfCrutan.Domain.Games.Resources;

namespace SettlersOfCrutan.Application.Games.Commands.Build;

public record BuyDevelopmentCardCommand(GameId GameId) : IGameCommand<DevelopmentCardType>;

public sealed class BuyDevelopmentCardCommandHandler(
    IGameRepository gameRepository,
    ICurrentUser currentUser,
    StandardPriceCalculator priceCalculator) : ICommandHandler<BuyDevelopmentCardCommand, DevelopmentCardType>
{
    private readonly IGameRepository _gameRepository = gameRepository;
    private readonly ICurrentUser _currentUser = currentUser;
    private readonly StandardPriceCalculator _priceCalculator = priceCalculator;

    public async Task<Result<DevelopmentCardType>> Handle(BuyDevelopmentCardCommand command, CancellationToken ct = default)
    {
        var game = await _gameRepository.GetAsync(command.GameId, ct);
        if (game is null) return Result<DevelopmentCardType>.Failure(DomainError.NotFound);

        var actor = GamePlayerResolution.ResolveActor(game, await _currentUser.UserId());
        if (actor.IsFailure) return Result<DevelopmentCardType>.Failure(actor.Error);

        var result = game.BuyDevelopmentCard(_priceCalculator, actor.Value);
        if (result.IsFailure) return Result<DevelopmentCardType>.Failure(result.Error);

        var saved = await _gameRepository.SaveAsync(game, ct);
        if (!saved) return Result<DevelopmentCardType>.Failure(new Error("Persistence", "Failed to save game state"));

        return Result<DevelopmentCardType>.Success(result.Value);
    }
}
