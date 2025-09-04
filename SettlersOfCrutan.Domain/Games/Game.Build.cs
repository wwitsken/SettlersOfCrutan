using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.Games.Boards;
using SettlersOfCrutan.Domain.Games.Boards.Coordinates;
using SettlersOfCrutan.Domain.Games.DomainEvents;

namespace SettlersOfCrutan.Domain.Games;
public partial class Game
{
    public Result<(PopulationCenter, Road)> PlaceInitialAndEndTurn(PlayerId playerId, Vertex settlementVertex, Edge roadEdge)
    {
        if (CurrentPlayerId() != playerId) return Result.Failure<(PopulationCenter, Road)>(DomainErrors.DomainError.WrongTurn);

        var result = Board.PlaceInitialSettlementAndRoad(playerId, settlementVertex, roadEdge);

        if (result.IsFailure) return Result.Failure<(PopulationCenter, Road)>(result.Error);

        AddDomainEvent(new InitialSettlementAndRoadPlaceDomainEvent(Id, playerId, result.Value.Item1, result.Value.Item2));

        EndTurn(playerId);

        return result;
    }
}
