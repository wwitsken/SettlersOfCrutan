using SettlersOfCrutan.Domain.Core;

namespace SettlersOfCrutan.Domain.Specifications.DiscardHalf;

public class DiscardAmountMustBeCorrect : ISpecification<DiscardHalfContext>
{
    public Result<Nothing> IsSatisfiedBy(DiscardHalfContext context) =>
        context.Requirement is not null && context.DiscardTotal == context.Requirement.ResourceAmount
            ? Result.Success()
            : Result.Failure(DomainError.IncorrectDiscardAmount);
}
