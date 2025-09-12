using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.DomainErrors;
using SettlersOfCrutan.Domain.Games.DomainEvents;
using SettlersOfCrutan.Domain.Games.Resources;

namespace SettlersOfCrutan.Domain.Games;
public partial class Game
{
    public List<PlayerId> PlayersNeedingToDiscardHalf => [.. _discardHalfRequirements.Select(r => r.PlayerId)];

    public Result<Nothing> DiscardHalf(PlayerId playerId, List<ResourceCardAmount> discards)
    {
        if (GamePhase != GamePhase.DiscardHalf)
            return Result.Failure(DomainError.CannotDiscardInCurrentPhase);

        var req = _discardHalfRequirements.FirstOrDefault(r => r.PlayerId.Equals(playerId));
        if (req is null) return Result.Failure(DomainError.PlayerNotRequiredToDiscard);
        if (discards is null || discards.Count == 0) return Result.Failure(DomainError.InvalidDiscardsPayload);

        int toDiscardTotal = discards.Sum(ra => Math.Max(0, ra.Quantity));
        if (toDiscardTotal != req.ResourceAmount) return Result.Failure(DomainError.IncorrectDiscardAmount);

        var player = Players.FirstOrDefault(p => p.Id.Equals(playerId));
        if (player is null) return Result.Failure(DomainError.NotFound);

        if (!player.HasAtLeast(discards))
            return Result.Failure(DomainError.PlayerInsufficientResourcesToDiscard);

        foreach (var (type, amount) in discards)
        {
            player.SubtractResource(type, amount);
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
