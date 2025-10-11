using SettlersOfCrutan.Application.Abstractions;
using SettlersOfCrutan.Application.Games.DTOs;
using SettlersOfCrutan.Application.Games.Policies;
using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.DomainErrors;
using SettlersOfCrutan.Domain.Games;
using SettlersOfCrutan.Domain.Games.Boards.Coordinates;

namespace SettlersOfCrutan.Application.Games.Commands.Build;

public record BuildRoadCommand(Guid GameId, string PlayerId, EdgeDto Edge) : ICommand;

public sealed class BuildRoadCommandHandler(IGameRepository gameRepository, StandardPriceCalculator priceCalculator) : ICommandHandler<BuildRoadCommand>
{
    private readonly IGameRepository _gameRepository = gameRepository;
    private readonly StandardPriceCalculator _priceCalculator = priceCalculator;

    public async Task<Result<Nothing>> Handle(BuildRoadCommand command, CancellationToken ct = default)
    {
        var game = await _gameRepository.GetAsync(new GameId() { Value = command.GameId }, ct);
        if (game is null) return Result.Failure(DomainError.NotFound);

        Edge edge = new(new HexCoord(
            command.Edge.HexCoord1.Q,
            command.Edge.HexCoord1.R,
            command.Edge.HexCoord1.S), new HexCoord(
            command.Edge.HexCoord2.Q,
            command.Edge.HexCoord2.R,
            command.Edge.HexCoord2.S));

        var result = game.BuildRoad(_priceCalculator, new() { Value = command.PlayerId }, edge);
        if (result.IsFailure) return Result.Failure(result.Error);

        var saved = await _gameRepository.SaveAsync(game, ct);
        return saved ? Result.Success() : Result.Failure(DomainError.InvalidOperation);
    }
}
