using SettlersOfCrutan.Domain.Core;

namespace SettlersOfCrutan.Domain.Specifications.BuildRoad;

public class MustBeCurrentPlayerTurn : ISpecification<BuildRoadContext>
{
    public Result<Nothing> IsSatisfiedBy(BuildRoadContext context) =>
        context.CurrentPlayerId == context.ActingPlayerId
            ? Result.Success()
            : Result.Failure(DomainError.WrongTurn);
}
