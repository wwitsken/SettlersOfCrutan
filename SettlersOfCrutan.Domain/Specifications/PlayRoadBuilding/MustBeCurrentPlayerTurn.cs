using SettlersOfCrutan.Domain.Core;

namespace SettlersOfCrutan.Domain.Specifications.PlayRoadBuilding;

public class MustBeCurrentPlayerTurn : ISpecification<PlayRoadBuildingContext>
{
    public Result<Nothing> IsSatisfiedBy(PlayRoadBuildingContext context) =>
        context.CurrentPlayerId == context.ActingPlayerId
            ? Result.Success()
            : Result.Failure(DomainError.WrongTurn);
}
