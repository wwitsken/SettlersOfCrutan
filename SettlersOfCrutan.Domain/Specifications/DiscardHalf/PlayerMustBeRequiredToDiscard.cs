using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.DomainErrors;

namespace SettlersOfCrutan.Domain.Specifications.DiscardHalf;

public class PlayerMustBeRequiredToDiscard : ISpecification<DiscardHalfContext>
{
    public Result<Nothing> IsSatisfiedBy(DiscardHalfContext context) =>
        context.Requirement is not null
            ? Result.Success()
            : Result.Failure(DomainError.PlayerNotRequiredToDiscard);
}
