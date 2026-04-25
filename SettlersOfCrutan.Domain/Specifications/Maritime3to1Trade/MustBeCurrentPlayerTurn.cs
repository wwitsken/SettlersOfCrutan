using SettlersOfCrutan.Domain.Core;

namespace SettlersOfCrutan.Domain.Specifications.Maritime3to1Trade;

public class MustBeCurrentPlayerTurn : ISpecification<Maritime3to1TradeContext>
{
    public Result<Nothing> IsSatisfiedBy(Maritime3to1TradeContext context) =>
        context.CurrentPlayerId == context.ActingPlayerId
            ? Result.Success()
            : Result.Failure(DomainError.WrongTurn);
}
