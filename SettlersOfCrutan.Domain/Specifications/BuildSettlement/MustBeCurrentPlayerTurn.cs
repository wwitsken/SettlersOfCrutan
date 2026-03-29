using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.DomainErrors;

namespace SettlersOfCrutan.Domain.Specifications.BuildSettlement;

public class MustBeCurrentPlayerTurn : ISpecification<BuildSettlementContext>
{
    public Result<Nothing> IsSatisfiedBy(BuildSettlementContext context) =>
        context.CurrentPlayerId == context.ActingPlayerId
            ? Result.Success()
            : Result.Failure(DomainError.WrongTurn);
}
