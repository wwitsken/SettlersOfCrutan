using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.Games;

namespace SettlersOfCrutan.Domain.Specifications.BuyDevelopmentCard;

public class GameMustBeInTradeBuildPhase : ISpecification<BuyDevelopmentCardContext>
{
    public Result<Nothing> IsSatisfiedBy(BuyDevelopmentCardContext context) =>
        context.GamePhase == GamePhase.TradeBuild
            ? Result.Success()
            : Result.Failure(DomainError.WrongGamePhase);
}
