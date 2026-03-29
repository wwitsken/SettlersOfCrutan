using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.DomainErrors;
using SettlersOfCrutan.Domain.Games;

namespace SettlersOfCrutan.Domain.Specifications.PlayRoadBuilding;

public class GameMustBeInTradeBuildPhase : ISpecification<PlayRoadBuildingContext>
{
    public Result<Nothing> IsSatisfiedBy(PlayRoadBuildingContext context) =>
        context.GamePhase == GamePhase.TradeBuild
            ? Result.Success()
            : Result.Failure(DomainError.WrongGamePhase);
}
