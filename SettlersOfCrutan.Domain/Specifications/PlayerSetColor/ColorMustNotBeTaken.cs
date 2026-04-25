using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.Games;

namespace SettlersOfCrutan.Domain.Specifications.PlayerSetColor;

public class ColorMustNotBeTaken : ISpecification<PlayerSetColorContext>
{
    public Result<Nothing> IsSatisfiedBy(PlayerSetColorContext context)
    {
        bool taken = context.AllPlayers.Any(x =>
            x.Color == context.Color && x.Id != context.PlayerId && context.Color != PlayerColor.None);
        return taken ? Result.Failure(DomainError.ColorTaken) : Result.Success();
    }
}
