using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.Games.Resources;

namespace SettlersOfCrutan.Domain.Specifications.PlayYearOfPlenty;

public class PlayerMustHaveYearOfPlentyCard : ISpecification<PlayYearOfPlentyContext>
{
    public Result<Nothing> IsSatisfiedBy(PlayYearOfPlentyContext context) =>
        context.ActingPlayer.CanUseDevCard(DevelopmentCardType.YearOfPlenty)
            ? Result.Success()
            : Result.Failure(DomainError.MissingYearOfPlentyCard);
}
