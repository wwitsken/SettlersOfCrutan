using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.Games;

namespace SettlersOfCrutan.Domain.Specifications.PlayYearOfPlenty;

public class GameMustBeInTradeBuildPhase : ISpecification<PlayYearOfPlentyContext>
{
    public Result<Nothing> IsSatisfiedBy(PlayYearOfPlentyContext context) =>
        context.GamePhase == GamePhase.TradeBuild
            ? Result.Success()
            : Result.Failure(DomainError.WrongGamePhase);
}
