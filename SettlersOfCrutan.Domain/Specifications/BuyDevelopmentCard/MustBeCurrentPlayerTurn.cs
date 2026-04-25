using SettlersOfCrutan.Domain.Core;

namespace SettlersOfCrutan.Domain.Specifications.BuyDevelopmentCard;

public class MustBeCurrentPlayerTurn : ISpecification<BuyDevelopmentCardContext>
{
    public Result<Nothing> IsSatisfiedBy(BuyDevelopmentCardContext context) =>
        context.CurrentPlayerId == context.ActingPlayerId
            ? Result.Success()
            : Result.Failure(DomainError.WrongTurn);
}
