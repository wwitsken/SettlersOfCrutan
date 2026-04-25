using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.Games;

namespace SettlersOfCrutan.Domain.Specifications.ProposeTrade;

public class GameMustBeInTradeBuildPhase : ISpecification<ProposeTradeContext>
{
    public Result<Nothing> IsSatisfiedBy(ProposeTradeContext context) =>
        context.GamePhase == GamePhase.TradeBuild
            ? Result.Success()
            : Result.Failure(DomainError.WrongGamePhase);
}
