using SettlersOfCrutan.Application.Abstractions;
using SettlersOfCrutan.Application.Games.Policies;
using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.DomainErrors;
using SettlersOfCrutan.Domain.Games.Resources;

namespace SettlersOfCrutan.Application.Games.Commands.Build;

public record BuyDevelopmentCardCommand(Guid GameId, string PlayerId) : ICommand<DevelopmentCardType>;

public sealed class BuyDevelopmentCardCommandHandler(IGameRepository gameRepository, StandardPriceCalculator priceCalculator)
    : ICommandHandler<BuyDevelopmentCardCommand, DevelopmentCardType>
{
    private readonly IGameRepository _gameRepository = gameRepository;
    private readonly StandardPriceCalculator _priceCalculator = priceCalculator;

    public async Task<Result<DevelopmentCardType>> Handle(BuyDevelopmentCardCommand command, CancellationToken ct = default)
    {
        var game = await _gameRepository.GetAsync(new() { Value = command.GameId }, ct);
        if (game is null) return Result<DevelopmentCardType>.Failure(DomainError.NotFound);

        var result = game.BuyDevelopmentCard(_priceCalculator, new() { Value = command.PlayerId });
        if (result.IsFailure) return Result<DevelopmentCardType>.Failure(result.Error);

        var saved = await _gameRepository.SaveAsync(game, ct);
        return saved ? Result<DevelopmentCardType>.Success(result.Value) : Result<DevelopmentCardType>.Failure(DomainError.InvalidOperation);
    }
}
