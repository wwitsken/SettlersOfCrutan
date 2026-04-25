using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.Games.Resources;

namespace SettlersOfCrutan.Domain.Specifications.PlayYearOfPlenty;

public class BankMustHaveRequestedResources : ISpecification<PlayYearOfPlentyContext>
{
    public Result<Nothing> IsSatisfiedBy(PlayYearOfPlentyContext context)
    {
        if (context.Type1 == context.Type2)
        {
            return context.BankResourceHand.HasAtLeast(new ResourceCardAmount(context.Type1, 2))
                ? Result.Success()
                : Result.Failure(DomainError.InsufficientResources);
        }

        return context.BankResourceHand.HasAtLeast(new ResourceCardAmount(context.Type1, 1))
            && context.BankResourceHand.HasAtLeast(new ResourceCardAmount(context.Type2, 1))
            ? Result.Success()
            : Result.Failure(DomainError.InsufficientResources);
    }
}
