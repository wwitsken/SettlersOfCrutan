using SettlersOfCrutan.Domain.Core;

namespace SettlersOfCrutan.Domain.Specifications.DiscardHalf;

public class PlayerMustHaveResourcesToDiscard : ISpecification<DiscardHalfContext>
{
    public Result<Nothing> IsSatisfiedBy(DiscardHalfContext context) =>
        context.Player is not null && context.Discards is not null && context.Player.HasAtLeast(context.Discards)
            ? Result.Success()
            : Result.Failure(DomainError.PlayerInsufficientResourcesToDiscard);
}
