using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.Games.Resources;

namespace SettlersOfCrutan.Domain.Specifications.PlayRoadBuilding;

public class PlayerMustHaveRoadBuildingCard : ISpecification<PlayRoadBuildingContext>
{
    public Result<Nothing> IsSatisfiedBy(PlayRoadBuildingContext context) =>
        context.ActingPlayer.CanUseDevCard(DevelopmentCardType.RoadBuilding)
            ? Result.Success()
            : Result.Failure(DomainError.CannotPlayRoadBuilding);
}
