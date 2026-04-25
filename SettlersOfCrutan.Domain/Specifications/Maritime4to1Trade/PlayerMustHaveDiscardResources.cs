using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.Games.Resources;

namespace SettlersOfCrutan.Domain.Specifications.Maritime4to1Trade;

public class PlayerMustHaveDiscardResources : ISpecification<Maritime4to1TradeContext>
{
    public Result<Nothing> IsSatisfiedBy(Maritime4to1TradeContext context) =>
        context.ActingPlayer.HasAtLeast(new ResourceCardAmount(context.DiscardResource, 4))
            ? Result.Success()
            : Result.Failure(DomainError.InsufficientResources);
}
