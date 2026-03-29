using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.DomainErrors;
using SettlersOfCrutan.Domain.Games.Resources;

namespace SettlersOfCrutan.Domain.Specifications.Maritime3to1Trade;

public class PlayerMustHaveDiscardResources : ISpecification<Maritime3to1TradeContext>
{
    public Result<Nothing> IsSatisfiedBy(Maritime3to1TradeContext context) =>
        context.ActingPlayer.HasAtLeast(new ResourceCardAmount(context.DiscardResource, 3))
            ? Result.Success()
            : Result.Failure(DomainError.InsufficientResources);
}
