using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.Games.DomainEvents;

namespace SettlersOfCrutan.Domain.Games;
public partial class Game
{
    public List<PlayerId> PlayersNeedingToDiscardHalf => [.. DiscardHalfRequirements.Select(r => r.PlayerId)];

    public Result<Nothing> DiscardHalf(PlayerId playerId, List<ResourceAmount> discards)
    {
        if (GamePhase != GamePhase.DiscardHalf)
            return Result.Failure(new Error("Discard", "Cannot discard in the current game phase"));

        var req = DiscardHalfRequirements.FirstOrDefault(r => r.PlayerId.Equals(playerId));
        if (req is null) return Result.Failure(new Error("PlayerNotRequired", "Player not required to discard"));
        if (discards is null || discards.Count == 0) return Result.Failure(new Error("InvalidDiscardsPayload", "Invalid discards payload"));

        int toDiscardTotal = discards.Sum(ra => Math.Max(0, ra.Quantity));
        if (toDiscardTotal != req.ResourceAmount) return Result.Failure(new Error("IncorrectDiscardAmount", "Incorrect discard amount"));

        var player = Players.FirstOrDefault(p => p.Id.Equals(playerId));
        if (player is null) return Result.Failure(DomainErrors.DomainError.NotFound);
        player.ResourceBag ??= new ResourceBag();

        foreach (var (type, amount) in discards)
        {
            if (amount < 0)
                return Result.Failure(new Error("NegativeDiscardAmount", "Negative discard amount"));
            if (!player.ResourceBag.HasAtLeast(type, amount))
                return Result.Failure(new Error("InsufficientCards", "Insufficient cards to discard"));
        }
        foreach (var (type, amount) in discards)
        {
            player.ResourceBag.SubtractResource(type, amount);
        }

        DiscardHalfRequirements.Remove(req);
        AddDomainEvent(new PlayerDiscardedHalfDomainEvent(Id, playerId, discards));

        if (DiscardHalfRequirements.Count == 0)
        {
            GamePhase = GamePhase.ResolveRobber;
            AddDomainEvent(new DiscardHalfCompleteDomainEvent(Id));
        }

        return Result.Success();
    }
}
