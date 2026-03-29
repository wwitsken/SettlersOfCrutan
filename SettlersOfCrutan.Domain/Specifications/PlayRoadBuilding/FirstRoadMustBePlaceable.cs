using SettlersOfCrutan.Domain.Core;

namespace SettlersOfCrutan.Domain.Specifications.PlayRoadBuilding;

public class FirstRoadMustBePlaceable : ISpecification<PlayRoadBuildingContext>
{
    public Result<Nothing> IsSatisfiedBy(PlayRoadBuildingContext context) =>
        context.Board.CanPlaceRoad(context.ActingPlayerId, context.Edge1);
}
