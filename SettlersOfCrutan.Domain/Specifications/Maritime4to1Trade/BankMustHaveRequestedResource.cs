using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.Games.Resources;

namespace SettlersOfCrutan.Domain.Specifications.Maritime4to1Trade;

public class BankMustHaveRequestedResource : ISpecification<Maritime4to1TradeContext>
{
    public Result<Nothing> IsSatisfiedBy(Maritime4to1TradeContext context) =>
        context.BankResourceHand.HasAtLeast(new ResourceCardAmount(context.RequestResource, 1))
            ? Result.Success()
            : Result.Failure(DomainError.InsufficientResources);
}
