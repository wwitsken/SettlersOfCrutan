using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.Games.Resources;

namespace SettlersOfCrutan.Domain.Specifications.Maritime3to1Trade;

public class BankMustHaveRequestedResource : ISpecification<Maritime3to1TradeContext>
{
    public Result<Nothing> IsSatisfiedBy(Maritime3to1TradeContext context) =>
        context.BankResourceHand.HasAtLeast(new ResourceCardAmount(context.RequestResource, 1))
            ? Result.Success()
            : Result.Failure(DomainError.InsufficientResources);
}
