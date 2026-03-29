using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.DomainErrors;

namespace SettlersOfCrutan.Domain.Specifications.PlayerSetReady;

public class PlayerMustHaveNameBeforeReady : ISpecification<PlayerSetReadyContext>
{
    public Result<Nothing> IsSatisfiedBy(PlayerSetReadyContext context) =>
        context.Player is not null && string.IsNullOrEmpty(context.Player.DisplayName) && context.Ready
            ? Result.Failure(DomainError.NameMustSetBeforeReady)
            : Result.Success();
}
