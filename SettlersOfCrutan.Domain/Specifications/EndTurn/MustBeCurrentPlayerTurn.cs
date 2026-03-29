using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.DomainErrors;

namespace SettlersOfCrutan.Domain.Specifications.EndTurn;

public class MustBeCurrentPlayerTurn : ISpecification<EndTurnContext>
{
    public Result<Nothing> IsSatisfiedBy(EndTurnContext context) =>
        context.CurrentPlayerId == context.ActingPlayerId
            ? Result.Success()
            : Result.Failure(DomainError.WrongTurn);
}
