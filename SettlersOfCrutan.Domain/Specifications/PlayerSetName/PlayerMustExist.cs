using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.DomainErrors;

namespace SettlersOfCrutan.Domain.Specifications.PlayerSetName;

public class PlayerMustExist : ISpecification<PlayerSetNameContext>
{
    public Result<Nothing> IsSatisfiedBy(PlayerSetNameContext context) =>
        context.Player is not null
            ? Result.Success()
            : Result.Failure(DomainError.PlayerNotFound(context.GameId, context.PlayerId));
}
