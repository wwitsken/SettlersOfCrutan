using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.Games;

namespace SettlersOfCrutan.Domain.Specifications.BuildSettlement;

public class GameMustBeInTradeBuildPhase : ISpecification<BuildSettlementContext>
{
    public Result<Nothing> IsSatisfiedBy(BuildSettlementContext context) =>
        context.GamePhase == GamePhase.TradeBuild
            ? Result.Success()
            : Result.Failure(DomainError.WrongGamePhase);
}
