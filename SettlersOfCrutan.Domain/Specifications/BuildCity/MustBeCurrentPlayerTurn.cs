using SettlersOfCrutan.Domain.Core;

namespace SettlersOfCrutan.Domain.Specifications.BuildCity;

public class MustBeCurrentPlayerTurn : ISpecification<BuildCityContext>
{
    public Result<Nothing> IsSatisfiedBy(BuildCityContext context) =>
        context.CurrentPlayerId == context.ActingPlayerId
            ? Result.Success()
            : Result.Failure(DomainError.WrongTurn);
}
