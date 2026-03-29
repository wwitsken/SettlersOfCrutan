using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.DomainErrors;

namespace SettlersOfCrutan.Domain.Specifications.BuildCity;

public class PlayerMustAffordCity : ISpecification<BuildCityContext>
{
    public Result<Nothing> IsSatisfiedBy(BuildCityContext context) =>
        context.ActingPlayer.CanPayResources(context.Cost)
            ? Result.Success()
            : Result.Failure(DomainError.InsufficientResources);
}
