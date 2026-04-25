using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.Games;

namespace SettlersOfCrutan.Domain.Specifications.RollAndResolveProduction;

public class GameMustBeInRollDicePhase : ISpecification<RollAndResolveProductionContext>
{
    public Result<Nothing> IsSatisfiedBy(RollAndResolveProductionContext context) =>
        context.GamePhase == GamePhase.RollDice
            ? Result.Success()
            : Result.Failure(DomainError.CannotRollInCurrentPhase);
}
