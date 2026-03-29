using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.DomainErrors;

namespace SettlersOfCrutan.Domain.Specifications.Maritime3to1Trade;

public class MustBeCurrentPlayerTurn : ISpecification<Maritime3to1TradeContext>
{
    public Result<Nothing> IsSatisfiedBy(Maritime3to1TradeContext context) =>
        context.CurrentPlayerId == context.ActingPlayerId
            ? Result.Success()
            : Result.Failure(DomainError.WrongTurn);
}
