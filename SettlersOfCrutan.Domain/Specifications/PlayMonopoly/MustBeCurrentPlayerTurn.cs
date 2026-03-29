using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.DomainErrors;

namespace SettlersOfCrutan.Domain.Specifications.PlayMonopoly;

public class MustBeCurrentPlayerTurn : ISpecification<PlayMonopolyContext>
{
    public Result<Nothing> IsSatisfiedBy(PlayMonopolyContext context) =>
        context.CurrentPlayerId == context.ActingPlayerId
            ? Result.Success()
            : Result.Failure(DomainError.WrongTurn);
}
