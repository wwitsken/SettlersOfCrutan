using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.DomainErrors;
using SettlersOfCrutan.Domain.Games;

namespace SettlersOfCrutan.Domain.Specifications.EndTurn;

public class GameMustBeInSetupOrTradeBuild : ISpecification<EndTurnContext>
{
    public Result<Nothing> IsSatisfiedBy(EndTurnContext context) =>
        context.GamePhase == GamePhase.Setup || context.GamePhase == GamePhase.TradeBuild
            ? Result.Success()
            : Result.Failure(DomainError.WrongGamePhase);
}
