using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.Games.Resources;

namespace SettlersOfCrutan.Domain.Specifications.BuildSettlement;

public class PlayerMustHaveSettlementPiece : ISpecification<BuildSettlementContext>
{
    public Result<Nothing> IsSatisfiedBy(BuildSettlementContext context) =>
        context.ActingPlayer.CanConsumePiece(BuildableType.Settlement)
            ? Result.Success()
            : Result.Failure(DomainError.MissingSettlement);
}
