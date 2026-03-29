using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.DomainErrors;
using SettlersOfCrutan.Domain.Games;

namespace SettlersOfCrutan.Domain.Specifications.Maritime2to1Trade;

public class GameMustBeInTradeBuildPhase : ISpecification<Maritime2to1TradeContext>
{
    public Result<Nothing> IsSatisfiedBy(Maritime2to1TradeContext context) =>
        context.GamePhase == GamePhase.TradeBuild
            ? Result.Success()
            : Result.Failure(DomainError.WrongGamePhase);
}
