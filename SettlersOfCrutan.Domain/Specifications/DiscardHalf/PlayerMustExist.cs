using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.DomainErrors;

namespace SettlersOfCrutan.Domain.Specifications.DiscardHalf;

public class PlayerMustExist : ISpecification<DiscardHalfContext>
{
    public Result<Nothing> IsSatisfiedBy(DiscardHalfContext context) =>
        context.Player is not null
            ? Result.Success()
            : Result.Failure(DomainError.NotFound);
}
