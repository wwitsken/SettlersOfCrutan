using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.DomainErrors;

namespace SettlersOfCrutan.Domain.Specifications.PlayYearOfPlenty;

public class MustBeCurrentPlayerTurn : ISpecification<PlayYearOfPlentyContext>
{
    public Result<Nothing> IsSatisfiedBy(PlayYearOfPlentyContext context) =>
        context.CurrentPlayerId == context.ActingPlayerId
            ? Result.Success()
            : Result.Failure(DomainError.WrongTurn);
}
