using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.DomainErrors;
using SettlersOfCrutan.Domain.Games.Resources;

namespace SettlersOfCrutan.Domain.Specifications.PlayMonopoly;

public class PlayerMustHaveMonopolyCard : ISpecification<PlayMonopolyContext>
{
    public Result<Nothing> IsSatisfiedBy(PlayMonopolyContext context) =>
        context.ActingPlayer.CanUseDevCard(DevelopmentCardType.Monopoly)
            ? Result.Success()
            : Result.Failure(DomainError.MissingMonopolyCard);
}
