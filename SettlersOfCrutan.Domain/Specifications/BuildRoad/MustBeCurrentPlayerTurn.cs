using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.DomainErrors;

namespace SettlersOfCrutan.Domain.Specifications.BuildRoad;

public class MustBeCurrentPlayerTurn : ISpecification<BuildRoadContext>
{
    public Result<Nothing> IsSatisfiedBy(BuildRoadContext context) =>
        context.CurrentPlayerId == context.ActingPlayerId
            ? Result.Success()
            : Result.Failure(DomainError.WrongTurn);
}
