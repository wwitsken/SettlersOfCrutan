using SettlersOfCrutan.Domain.Core;

namespace SettlersOfCrutan.Domain.Specifications.BuyDevelopmentCard;

public class BankMustHaveDevelopmentCards : ISpecification<BuyDevelopmentCardContext>
{
    public Result<Nothing> IsSatisfiedBy(BuyDevelopmentCardContext context) =>
        context.BankDevCardCount > 0
            ? Result.Success()
            : Result.Failure(DomainError.InsufficientBankDevelopmentCards);
}
