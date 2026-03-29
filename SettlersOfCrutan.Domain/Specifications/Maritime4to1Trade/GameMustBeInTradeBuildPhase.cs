using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.DomainErrors;
using SettlersOfCrutan.Domain.Games;

namespace SettlersOfCrutan.Domain.Specifications.Maritime4to1Trade;

public class GameMustBeInTradeBuildPhase : ISpecification<Maritime4to1TradeContext>
{
    public Result<Nothing> IsSatisfiedBy(Maritime4to1TradeContext context) =>
        context.GamePhase == GamePhase.TradeBuild
            ? Result.Success()
            : Result.Failure(DomainError.WrongGamePhase);
}
