using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.Games.Resources;

namespace SettlersOfCrutan.Domain.Specifications.PlayRoadBuilding;

public class PlayerMustHaveTwoRoadPieces : ISpecification<PlayRoadBuildingContext>
{
    public Result<Nothing> IsSatisfiedBy(PlayRoadBuildingContext context) =>
        context.ActingPlayer.CanConsumePiece(BuildableType.Road, 2)
            ? Result.Success()
            : Result.Failure(DomainError.CannotPlayRoadBuilding);
}
