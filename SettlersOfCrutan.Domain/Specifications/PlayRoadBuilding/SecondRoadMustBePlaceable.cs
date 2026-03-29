using SettlersOfCrutan.Domain.Core;

namespace SettlersOfCrutan.Domain.Specifications.PlayRoadBuilding;

public class SecondRoadMustBePlaceable : ISpecification<PlayRoadBuildingContext>
{
    public Result<Nothing> IsSatisfiedBy(PlayRoadBuildingContext context) =>
        context.Board.CanPlaceRoad(
            context.ActingPlayerId,
            context.Edge2,
            hypotheticalExtraOwnerRoads: [context.Edge1.Normalize()]);
}
