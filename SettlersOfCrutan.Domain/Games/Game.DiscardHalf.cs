using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.Games.DomainEvents;
using SettlersOfCrutan.Domain.Games.Resources;

namespace SettlersOfCrutan.Domain.Games;
public partial class Game
{
    public List<PlayerId> PlayersNeedingToDiscardHalf => [.. _discardHalfRequirements.Select(r => r.PlayerId)];

    public Result<Nothing> DiscardHalf(PlayerId playerId, List<ResourceCardAmount> discards)
    {
        if (GamePhase != GamePhase.DiscardHalf)
            return Result.Failure(new Error("Discard", "Cannot discard in the current game phase"));

        var req = _discardHalfRequirements.FirstOrDefault(r => r.PlayerId.Equals(playerId));
        if (req is null) return Result.Failure(new Error("PlayerNotRequired", "Player not required to discard"));
        if (discards is null || discards.Count == 0) return Result.Failure(new Error("InvalidDiscardsPayload", "Invalid discards payload"));

        int toDiscardTotal = discards.Sum(ra => Math.Max(0, ra.Quantity));
        if (toDiscardTotal != req.ResourceAmount) return Result.Failure(new Error("IncorrectDiscardAmount", "Incorrect discard amount"));

        var player = Players.FirstOrDefault(p => p.Id.Equals(playerId));
        if (player is null) return Result.Failure(DomainErrors.DomainError.NotFound);

        if (!player.ResourceHand.HasAtLeast(discards))
            return Result.Failure(new Error("InsufficientResources", "Player does not have required resources to discard"));

        foreach (var (type, amount) in discards)
        {
            player.ResourceHand.Subtract(type, amount);
            BankResourceHand.Add(type, amount); // return to bank
        }

        _discardHalfRequirements.Remove(req);
        AddDomainEvent(new PlayerDiscardedHalfDomainEvent(Id, playerId, discards));

        if (_discardHalfRequirements.Count == 0)
        {
            GamePhase = GamePhase.ResolveRobber;
            AddDomainEvent(new DiscardHalfCompleteDomainEvent(Id));
        }

        return Result.Success();
    }
}
