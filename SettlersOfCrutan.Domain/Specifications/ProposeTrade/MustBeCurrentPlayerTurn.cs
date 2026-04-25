using SettlersOfCrutan.Domain.Core;

namespace SettlersOfCrutan.Domain.Specifications.ProposeTrade;

public class MustBeCurrentPlayerTurn : ISpecification<ProposeTradeContext>
{
    public Result<Nothing> IsSatisfiedBy(ProposeTradeContext context) =>
        context.CurrentPlayerId == context.ActingPlayerId
            ? Result.Success()
            : Result.Failure(DomainError.WrongTurn);
}
