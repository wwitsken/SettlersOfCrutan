using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.DomainErrors;
using SettlersOfCrutan.Domain.Games.Resources;

namespace SettlersOfCrutan.Domain.Specifications.BuildCity;

public class PlayerMustHaveCityPiece : ISpecification<BuildCityContext>
{
    public Result<Nothing> IsSatisfiedBy(BuildCityContext context) =>
        context.ActingPlayer.CanConsumePiece(BuildableType.City)
            ? Result.Success()
            : Result.Failure(DomainError.MissingCity);
}
