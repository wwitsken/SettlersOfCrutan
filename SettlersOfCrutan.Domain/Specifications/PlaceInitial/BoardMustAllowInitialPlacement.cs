using SettlersOfCrutan.Domain.Core;

namespace SettlersOfCrutan.Domain.Specifications.PlaceInitial;

public class BoardMustAllowInitialPlacement : ISpecification<PlaceInitialContext>
{
    public Result<Nothing> IsSatisfiedBy(PlaceInitialContext context) =>
        context.Board.CanPlaceInitialSettleAndRoad(context.SettlementVertex, context.RoadEdge);
}
