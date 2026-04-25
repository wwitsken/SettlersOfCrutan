using SettlersOfCrutan.Domain.Core;

namespace SettlersOfCrutan.Domain.Specifications.Maritime4to1Trade;

public class MustBeCurrentPlayerTurn : ISpecification<Maritime4to1TradeContext>
{
    public Result<Nothing> IsSatisfiedBy(Maritime4to1TradeContext context) =>
        context.CurrentPlayerId == context.ActingPlayerId
            ? Result.Success()
            : Result.Failure(DomainError.WrongTurn);
}
