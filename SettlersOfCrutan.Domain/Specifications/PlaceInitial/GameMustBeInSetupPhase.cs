using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.DomainErrors;
using SettlersOfCrutan.Domain.Games;

namespace SettlersOfCrutan.Domain.Specifications.PlaceInitial;

public class GameMustBeInSetupPhase : ISpecification<PlaceInitialContext>
{
    public Result<Nothing> IsSatisfiedBy(PlaceInitialContext context) =>
        context.GamePhase == GamePhase.Setup
            ? Result.Success()
            : Result.Failure(DomainError.WrongGamePhase);
}
