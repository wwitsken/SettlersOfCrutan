using SettlersOfCrutan.Domain.Core;

namespace SettlersOfCrutan.Domain.Specifications.Maritime2to1Trade;

public class MustBeCurrentPlayerTurn : ISpecification<Maritime2to1TradeContext>
{
    public Result<Nothing> IsSatisfiedBy(Maritime2to1TradeContext context) =>
        context.CurrentPlayerId == context.ActingPlayerId
            ? Result.Success()
            : Result.Failure(DomainError.WrongTurn);
}
