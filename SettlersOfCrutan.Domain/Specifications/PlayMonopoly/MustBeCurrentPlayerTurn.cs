using SettlersOfCrutan.Domain.Core;

namespace SettlersOfCrutan.Domain.Specifications.PlayMonopoly;

public class MustBeCurrentPlayerTurn : ISpecification<PlayMonopolyContext>
{
    public Result<Nothing> IsSatisfiedBy(PlayMonopolyContext context) =>
        context.CurrentPlayerId == context.ActingPlayerId
            ? Result.Success()
            : Result.Failure(DomainError.WrongTurn);
}
