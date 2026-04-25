using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.Games;

namespace SettlersOfCrutan.Domain.Specifications.PlayMonopoly;

public class GameMustBeInTradeBuildPhase : ISpecification<PlayMonopolyContext>
{
    public Result<Nothing> IsSatisfiedBy(PlayMonopolyContext context) =>
        context.GamePhase == GamePhase.TradeBuild
            ? Result.Success()
            : Result.Failure(DomainError.WrongGamePhase);
}
