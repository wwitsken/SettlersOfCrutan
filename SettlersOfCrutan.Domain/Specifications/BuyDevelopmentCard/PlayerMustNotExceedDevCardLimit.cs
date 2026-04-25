using SettlersOfCrutan.Domain.Core;

namespace SettlersOfCrutan.Domain.Specifications.BuyDevelopmentCard;

public class PlayerMustNotExceedDevCardLimit : ISpecification<BuyDevelopmentCardContext>
{
    public Result<Nothing> IsSatisfiedBy(BuyDevelopmentCardContext context) =>
        context.PlayerDevCardCount <= 3
            ? Result.Success()
            : Result.Failure(DomainError.TooManyDevelopmentCards);
}
