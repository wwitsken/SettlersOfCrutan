using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.DomainErrors;
using SettlersOfCrutan.Domain.Games.Resources;

namespace SettlersOfCrutan.Domain.Specifications.PlaceInitial;

public class PlayerMustHaveInitialPieces : ISpecification<PlaceInitialContext>
{
    public Result<Nothing> IsSatisfiedBy(PlaceInitialContext context) =>
        context.ActingPlayer.CanConsumePiece(BuildableType.Settlement)
            && context.ActingPlayer.CanConsumePiece(BuildableType.Road)
            ? Result.Success()
            : Result.Failure(DomainError.MissingInitialPieces);
}
