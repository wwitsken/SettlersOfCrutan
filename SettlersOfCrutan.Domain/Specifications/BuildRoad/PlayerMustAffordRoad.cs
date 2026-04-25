using SettlersOfCrutan.Domain.Core;

namespace SettlersOfCrutan.Domain.Specifications.BuildRoad;

public class PlayerMustAffordRoad : ISpecification<BuildRoadContext>
{
    public Result<Nothing> IsSatisfiedBy(BuildRoadContext context) =>
        context.ActingPlayer.CanPayResources(context.Cost)
            ? Result.Success()
            : Result.Failure(DomainError.InsufficientResources);
}
