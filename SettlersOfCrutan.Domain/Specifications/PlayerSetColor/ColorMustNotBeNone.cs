using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.DomainErrors;
using SettlersOfCrutan.Domain.Games;

namespace SettlersOfCrutan.Domain.Specifications.PlayerSetColor;

public class ColorMustNotBeNone : ISpecification<PlayerSetColorContext>
{
    public Result<Nothing> IsSatisfiedBy(PlayerSetColorContext context) =>
        context.Color != PlayerColor.None
            ? Result.Success()
            : Result.Failure(DomainError.InvalidColor);
}
