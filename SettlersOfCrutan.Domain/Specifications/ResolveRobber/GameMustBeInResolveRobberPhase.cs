using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.Games;

namespace SettlersOfCrutan.Domain.Specifications.ResolveRobber;

public class GameMustBeInResolveRobberPhase : ISpecification<ResolveRobberContext>
{
    public Result<Nothing> IsSatisfiedBy(ResolveRobberContext context) =>
        context.GamePhase == GamePhase.ResolveRobber
            ? Result.Success()
            : Result.Failure(DomainError.CannotResolveRobberInCurrentPhase);
}
