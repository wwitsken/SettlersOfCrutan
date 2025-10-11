using SettlersOfCrutan.Application.Abstractions;
using SettlersOfCrutan.Application.Games.Policies;
using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.DomainErrors;
using SettlersOfCrutan.Domain.Games;
using SettlersOfCrutan.Domain.Games.Boards;
using SettlersOfCrutan.Domain.Games.Boards.Coordinates;

namespace SettlersOfCrutan.Application.Games.Commands.Build;

public record UpgradeSettlementToCityCommand(GameId GameId, PlayerId PlayerId, Vertex Vertex) : ICommand<PopulationCenter>;

public sealed class UpgradeSettlementToCityCommandHandler(IGameRepository gameRepository, StandardPriceCalculator priceCalculator) : ICommandHandler<UpgradeSettlementToCityCommand, PopulationCenter>
{
    private readonly IGameRepository _gameRepository = gameRepository;
    private readonly StandardPriceCalculator _priceCalculator = priceCalculator;

    public async Task<Result<PopulationCenter>> Handle(UpgradeSettlementToCityCommand command, CancellationToken ct = default)
    {
        var game = await _gameRepository.GetAsync(command.GameId, ct);
        if (game is null) return Result<PopulationCenter>.Failure(DomainError.NotFound);

        var result = game.BuildCity(_priceCalculator, command.PlayerId, command.Vertex);
        if (result.IsFailure) return Result<PopulationCenter>.Failure(result.Error);

        var saved = await _gameRepository.SaveAsync(game, ct);
        return saved ? Result<PopulationCenter>.Success(result.Value) : Result<PopulationCenter>.Failure(DomainError.InvalidOperation);
    }
}
