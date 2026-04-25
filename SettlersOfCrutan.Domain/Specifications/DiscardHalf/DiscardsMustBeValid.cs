using SettlersOfCrutan.Domain.Core;

namespace SettlersOfCrutan.Domain.Specifications.DiscardHalf;

public class DiscardsMustBeValid : ISpecification<DiscardHalfContext>
{
    public Result<Nothing> IsSatisfiedBy(DiscardHalfContext context) =>
        context.Discards is not null && context.Discards.Count > 0
            ? Result.Success()
            : Result.Failure(DomainError.InvalidDiscardsPayload);
}
