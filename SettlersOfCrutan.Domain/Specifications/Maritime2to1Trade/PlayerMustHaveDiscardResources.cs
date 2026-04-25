using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.Games.Resources;

namespace SettlersOfCrutan.Domain.Specifications.Maritime2to1Trade;

public class PlayerMustHaveDiscardResources : ISpecification<Maritime2to1TradeContext>
{
    public Result<Nothing> IsSatisfiedBy(Maritime2to1TradeContext context) =>
        context.ActingPlayer.HasAtLeast(new ResourceCardAmount(context.DiscardResource, 2))
            ? Result.Success()
            : Result.Failure(DomainError.InsufficientResources);
}
