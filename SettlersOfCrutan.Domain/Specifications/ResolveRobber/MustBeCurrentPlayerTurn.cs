using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.DomainErrors;

namespace SettlersOfCrutan.Domain.Specifications.ResolveRobber;

public class MustBeCurrentPlayerTurn : ISpecification<ResolveRobberContext>
{
    public Result<Nothing> IsSatisfiedBy(ResolveRobberContext context) =>
        context.CurrentPlayerId == context.ActingPlayerId
            ? Result.Success()
            : Result.Failure(DomainError.WrongTurn);
}
