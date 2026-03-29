using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.DomainErrors;
using SettlersOfCrutan.Domain.Games;

namespace SettlersOfCrutan.Application;

/// <summary>
/// API endpoints pass <see cref="PlayerId.Create"/> with the authenticated account id (<see cref="Player.UserId"/>).
/// In-game identity for rules and board state is <see cref="Player.Id"/> (entity id).
/// </summary>
internal static class GamePlayerResolution
{
    public static Result<PlayerId> ResolveActor(Game game, PlayerId authenticatedIdentity) =>
        ResolveActor(game, authenticatedIdentity.Value);

    public static Result<PlayerId> ResolveActor(Game game, string authenticatedUserId)
    {
        var p = game.Players.FirstOrDefault(x => x.UserId == authenticatedUserId);
        return p is null
            ? Result.Failure<PlayerId>(DomainError.UserNotInGame(game.Id))
            : Result.Success(p.Id);
    }
}
