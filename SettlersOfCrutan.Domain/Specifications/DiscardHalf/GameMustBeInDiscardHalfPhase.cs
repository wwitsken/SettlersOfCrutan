using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.Games;

namespace SettlersOfCrutan.Domain.Specifications.DiscardHalf;

public class GameMustBeInDiscardHalfPhase : ISpecification<DiscardHalfContext>
{
    public Result<Nothing> IsSatisfiedBy(DiscardHalfContext context) =>
        context.GamePhase == GamePhase.DiscardHalf
            ? Result.Success()
            : Result.Failure(DomainError.CannotDiscardInCurrentPhase);
}
