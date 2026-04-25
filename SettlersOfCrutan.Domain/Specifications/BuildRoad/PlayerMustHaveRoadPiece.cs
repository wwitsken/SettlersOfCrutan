using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.Games.Resources;

namespace SettlersOfCrutan.Domain.Specifications.BuildRoad;

public class PlayerMustHaveRoadPiece : ISpecification<BuildRoadContext>
{
    public Result<Nothing> IsSatisfiedBy(BuildRoadContext context) =>
        context.ActingPlayer.CanConsumePiece(BuildableType.Road)
            ? Result.Success()
            : Result.Failure(DomainError.MissingRoad);
}
