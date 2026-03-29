using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.DomainErrors;
using SettlersOfCrutan.Domain.Games.Resources;

namespace SettlersOfCrutan.Domain.Specifications.PlayKnight;

public class PlayerMustHaveKnightCard : ISpecification<PlayKnightContext>
{
    public Result<Nothing> IsSatisfiedBy(PlayKnightContext context) =>
        context.ActingPlayer.CanUseDevCard(DevelopmentCardType.Knight)
            ? Result.Success()
            : Result.Failure(DomainError.MissingKnightCard);
}
