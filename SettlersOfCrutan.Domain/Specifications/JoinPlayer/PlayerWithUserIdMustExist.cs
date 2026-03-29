using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.DomainErrors;

namespace SettlersOfCrutan.Domain.Specifications.JoinPlayer;

public class PlayerWithUserIdMustExist : ISpecification<JoinPlayerContext>
{
    public Result<Nothing> IsSatisfiedBy(JoinPlayerContext context) =>
        context.Player is not null
            ? Result.Success()
            : Result.Failure(DomainError.UserNotInGame(context.GameId));
}
