using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.DomainErrors;
using SettlersOfCrutan.Domain.Games;

namespace SettlersOfCrutan.Domain.Specifications.PlayerSetReady;

public class PlayerMustHaveColorBeforeReady : ISpecification<PlayerSetReadyContext>
{
    public Result<Nothing> IsSatisfiedBy(PlayerSetReadyContext context) =>
        context.Player is not null && context.Player.Color == PlayerColor.None && context.Ready
            ? Result.Failure(DomainError.ColorMustSetBeforeReady)
            : Result.Success();
}
