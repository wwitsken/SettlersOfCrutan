using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.DomainErrors;

namespace SettlersOfCrutan.Domain.Specifications.PlaceInitial;

public class MustBeCurrentPlayerTurn : ISpecification<PlaceInitialContext>
{
    public Result<Nothing> IsSatisfiedBy(PlaceInitialContext context) =>
        context.CurrentPlayerId == context.ActingPlayerId
            ? Result.Success()
            : Result.Failure(DomainError.WrongTurn);
}
