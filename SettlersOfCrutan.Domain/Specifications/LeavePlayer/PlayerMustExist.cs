using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.DomainErrors;

namespace SettlersOfCrutan.Domain.Specifications.LeavePlayer;

public class PlayerMustExist : ISpecification<LeavePlayerContext>
{
    public Result<Nothing> IsSatisfiedBy(LeavePlayerContext context) =>
        context.Player is not null
            ? Result.Success()
            : Result.Failure(DomainError.PlayerNotFound(context.GameId, context.PlayerId));
}
