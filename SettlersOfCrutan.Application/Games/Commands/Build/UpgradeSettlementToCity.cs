using SettlersOfCrutan.Application.Abstractions;
using SettlersOfCrutan.Application.Games.DTOs;
using SettlersOfCrutan.Application.Games.Policies;
using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.DomainErrors;
using SettlersOfCrutan.Domain.Games.Boards.Coordinates;

namespace SettlersOfCrutan.Application.Games.Commands.Build;

public record UpgradeSettlementToCityCommand(Guid GameId, string PlayerId, VertexDto Vertex) : ICommand;

public sealed class UpgradeSettlementToCityCommandHandler(IGameRepository gameRepository, StandardPriceCalculator priceCalculator)
    : ICommandHandler<UpgradeSettlementToCityCommand>
{
    private readonly IGameRepository _gameRepository = gameRepository;
    private readonly StandardPriceCalculator _priceCalculator = priceCalculator;

    public async Task<Result<Nothing>> Handle(UpgradeSettlementToCityCommand command, CancellationToken ct = default)
    {
        var game = await _gameRepository.GetAsync(new() { Value = command.GameId }, ct);
        if (game is null) return Result<Nothing>.Failure(DomainError.NotFound);

        Vertex vertex = new()
        {
            HexCoord1 = new HexCoord
            {
                Q = command.Vertex.HexCoord1.Q,
                R = command.Vertex.HexCoord1.R,
                S = command.Vertex.HexCoord1.S
            },
            HexCoord2 = new HexCoord
            {
                Q = command.Vertex.HexCoord2.Q,
                R = command.Vertex.HexCoord2.R,
                S = command.Vertex.HexCoord2.S
            },
            HexCoord3 = new HexCoord
            {
                Q = command.Vertex.HexCoord3.Q,
                R = command.Vertex.HexCoord3.R,
                S = command.Vertex.HexCoord3.S
            }
        };

        var result = game.BuildCity(_priceCalculator, new() { Value = command.PlayerId }, vertex);
        if (result.IsFailure) return Result<Nothing>.Failure(result.Error);

        var saved = await _gameRepository.SaveAsync(game, ct);
        return saved ? Result<Nothing>.Success() : Result<Nothing>.Failure(DomainError.InvalidOperation);
    }
}
