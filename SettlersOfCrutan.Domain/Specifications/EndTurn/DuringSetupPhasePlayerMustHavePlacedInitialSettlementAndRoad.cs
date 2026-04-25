using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.Games;

namespace SettlersOfCrutan.Domain.Specifications.EndTurn;

internal class DuringSetupPhasePlayerMustHavePlacedInitialSettlementAndRoad : ISpecification<EndTurnContext>
{
    public Result<Nothing> IsSatisfiedBy(EndTurnContext context)
    {
        if (context.GamePhase != GamePhase.Setup)
            return Result.Success();

        // Setup is a snake draft: `Round` stays 1 until setup finishes. Use direction to know
        // whether this is the first pass (1 settlement + 1 road) or the return pass (2 each).
        var expectedPairs = context.PlayerDirection == PlayerDirection.Clockwise ? 1 : 2;
        if (context.PlayerRoads.Count != expectedPairs ||
            context.PlayerPopulationCenters.Count != expectedPairs)
            return Result.Failure(DomainError.CannotEndTurnWithoutPlacingInitialSettlementAndRoad);

        return Result.Success();
    }
}
