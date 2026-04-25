using SettlersOfCrutan.Domain.Core;

namespace SettlersOfCrutan.Domain.Specifications.PlayerSetReady;

public class PlayerMustExist : ISpecification<PlayerSetReadyContext>
{
    public Result<Nothing> IsSatisfiedBy(PlayerSetReadyContext context) =>
        context.Player is not null
            ? Result.Success()
            : Result.Failure(DomainError.PlayerNotFound(context.GameId, context.PlayerId));
}
