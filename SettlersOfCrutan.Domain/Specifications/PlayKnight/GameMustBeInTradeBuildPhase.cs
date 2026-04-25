using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.Games;

namespace SettlersOfCrutan.Domain.Specifications.PlayKnight;

public class GameMustBeInTradeBuildPhase : ISpecification<PlayKnightContext>
{
    public Result<Nothing> IsSatisfiedBy(PlayKnightContext context) =>
        context.GamePhase == GamePhase.TradeBuild
            ? Result.Success()
            : Result.Failure(DomainError.WrongGamePhase);
}
