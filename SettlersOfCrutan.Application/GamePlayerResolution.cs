using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.Games;
using SettlersOfCrutan.Domain.Users;

namespace SettlersOfCrutan.Application;

internal static class GamePlayerResolution
{
    public static Result<PlayerId> ResolveActor(Game game, UserId authenticatedUserId) =>
        game.ResolvePlayerFromUserId(authenticatedUserId);
}
