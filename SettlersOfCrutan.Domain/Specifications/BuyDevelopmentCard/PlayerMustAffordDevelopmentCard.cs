using SettlersOfCrutan.Domain.Core;

namespace SettlersOfCrutan.Domain.Specifications.BuyDevelopmentCard;

public class PlayerMustAffordDevelopmentCard : ISpecification<BuyDevelopmentCardContext>
{
    public Result<Nothing> IsSatisfiedBy(BuyDevelopmentCardContext context) =>
        context.ActingPlayer.CanPayResources(context.Cost)
            ? Result.Success()
            : Result.Failure(DomainError.InsufficientResources);
}
