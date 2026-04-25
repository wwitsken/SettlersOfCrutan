using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.Games;

namespace SettlersOfCrutan.Domain.Specifications.Maritime3to1Trade;

public class GameMustBeInTradeBuildPhase : ISpecification<Maritime3to1TradeContext>
{
    public Result<Nothing> IsSatisfiedBy(Maritime3to1TradeContext context) =>
        context.GamePhase == GamePhase.TradeBuild
            ? Result.Success()
            : Result.Failure(DomainError.WrongGamePhase);
}
