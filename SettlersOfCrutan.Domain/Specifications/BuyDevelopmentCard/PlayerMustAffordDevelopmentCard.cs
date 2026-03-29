using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.DomainErrors;

namespace SettlersOfCrutan.Domain.Specifications.BuyDevelopmentCard;

public class PlayerMustAffordDevelopmentCard : ISpecification<BuyDevelopmentCardContext>
{
    public Result<Nothing> IsSatisfiedBy(BuyDevelopmentCardContext context) =>
        context.ActingPlayer.CanPayResources(context.Cost)
            ? Result.Success()
            : Result.Failure(DomainError.InsufficientResources);
}
