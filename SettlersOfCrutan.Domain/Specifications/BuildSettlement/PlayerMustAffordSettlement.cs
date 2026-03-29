using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.DomainErrors;

namespace SettlersOfCrutan.Domain.Specifications.BuildSettlement;

public class PlayerMustAffordSettlement : ISpecification<BuildSettlementContext>
{
    public Result<Nothing> IsSatisfiedBy(BuildSettlementContext context) =>
        context.ActingPlayer.CanPayResources(context.Cost)
            ? Result.Success()
            : Result.Failure(DomainError.InsufficientResources);
}
