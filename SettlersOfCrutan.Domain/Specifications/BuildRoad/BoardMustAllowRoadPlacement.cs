using SettlersOfCrutan.Domain.Core;

namespace SettlersOfCrutan.Domain.Specifications.BuildRoad;

public class BoardMustAllowRoadPlacement : ISpecification<BuildRoadContext>
{
    public Result<Nothing> IsSatisfiedBy(BuildRoadContext context) =>
        context.Board.CanPlaceRoad(context.ActingPlayerId, context.Edge);
}
