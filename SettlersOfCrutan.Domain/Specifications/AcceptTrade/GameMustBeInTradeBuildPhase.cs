using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.Games;

namespace SettlersOfCrutan.Domain.Specifications.AcceptTrade;

public class GameMustBeInTradeBuildPhase : ISpecification<AcceptTradeContext>
{
    public Result<Nothing> IsSatisfiedBy(AcceptTradeContext context) =>
        context.GamePhase == GamePhase.TradeBuild
            ? Result.Success()
            : Result.Failure(DomainError.WrongGamePhase);
}
