using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.DomainErrors;

namespace SettlersOfCrutan.Domain.Specifications.RollAndResolveProduction;

public class MustBeCurrentPlayerTurn : ISpecification<RollAndResolveProductionContext>
{
    public Result<Nothing> IsSatisfiedBy(RollAndResolveProductionContext context) =>
        context.CurrentPlayerId == context.ActingPlayerId
            ? Result.Success()
            : Result.Failure(DomainError.WrongTurn);
}
