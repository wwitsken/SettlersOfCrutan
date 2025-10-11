using SettlersOfCrutan.Application.Abstractions;
using SettlersOfCrutan.Application.Games.Policies;
using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.DomainErrors;
using SettlersOfCrutan.Domain.Games;
using SettlersOfCrutan.Domain.Games.Boards;
using SettlersOfCrutan.Domain.Games.Boards.Coordinates;

namespace SettlersOfCrutan.Application.Games.Commands.Build;

public record BuildRoadCommand(GameId GameId, PlayerId PlayerId, Edge Edge) : ICommand<Road>;

public sealed class BuildRoadCommandHandler(IGameRepository gameRepository, StandardPriceCalculator priceCalculator) : ICommandHandler<BuildRoadCommand, Road>
{
    private readonly IGameRepository _gameRepository = gameRepository;
    private readonly StandardPriceCalculator _priceCalculator = priceCalculator;

    public async Task<Result<Road>> Handle(BuildRoadCommand command, CancellationToken ct = default)
    {
        var game = await _gameRepository.GetAsync(command.GameId, ct);
        if (game is null) return Result<Road>.Failure(DomainError.NotFound);

        var result = game.BuildRoad(_priceCalculator, command.PlayerId, command.Edge);
        if (result.IsFailure) return Result<Road>.Failure(result.Error);

        var saved = await _gameRepository.SaveAsync(game, ct);
        return saved ? Result<Road>.Success(result.Value) : Result<Road>.Failure(DomainError.InvalidOperation);
    }
}
