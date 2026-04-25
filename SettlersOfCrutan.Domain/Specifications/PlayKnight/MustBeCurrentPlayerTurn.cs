using SettlersOfCrutan.Domain.Core;

namespace SettlersOfCrutan.Domain.Specifications.PlayKnight;

public class MustBeCurrentPlayerTurn : ISpecification<PlayKnightContext>
{
    public Result<Nothing> IsSatisfiedBy(PlayKnightContext context) =>
        context.CurrentPlayerId == context.ActingPlayerId
            ? Result.Success()
            : Result.Failure(DomainError.WrongTurn);
}
