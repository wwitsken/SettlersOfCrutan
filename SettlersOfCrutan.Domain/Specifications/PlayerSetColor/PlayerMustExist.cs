using SettlersOfCrutan.Domain.Core;

namespace SettlersOfCrutan.Domain.Specifications.PlayerSetColor;

public class PlayerMustExist : ISpecification<PlayerSetColorContext>
{
    public Result<Nothing> IsSatisfiedBy(PlayerSetColorContext context) =>
        context.Player is not null
            ? Result.Success()
            : Result.Failure(DomainError.PlayerNotFound(context.GameId, context.PlayerId));
}
