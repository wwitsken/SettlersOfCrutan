using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.Games.DomainEvents;
using SettlersOfCrutan.Domain.Games.Resources;
using SettlersOfCrutan.Domain.Specifications;
using DiscardSpecs = SettlersOfCrutan.Domain.Specifications.DiscardHalf;

namespace SettlersOfCrutan.Domain.Games;
public partial class Game
{
    public List<PlayerId> PlayersNeedingToDiscardHalf => [.. _discardHalfRequirements.Select(r => r.PlayerId)];

    private static readonly ISpecification<DiscardSpecs.DiscardHalfContext>[] DiscardHalfSpecifications =
    [
        new DiscardSpecs.GameMustBeInDiscardHalfPhase(),
        new DiscardSpecs.PlayerMustBeRequiredToDiscard(),
        new DiscardSpecs.DiscardsMustBeValid(),
        new DiscardSpecs.DiscardAmountMustBeCorrect(),
        new DiscardSpecs.PlayerMustExist(),
        new DiscardSpecs.PlayerMustHaveResourcesToDiscard()
    ];

    public Result<Nothing> DiscardHalf(PlayerId playerId, List<ResourceCardAmount> discards)
    {
        var req = _discardHalfRequirements.FirstOrDefault(r => r.PlayerId.Equals(playerId));
        var player = Players.FirstOrDefault(p => p.Id.Equals(playerId));
        int discardTotal = discards?.Sum(ra => Math.Max(0, ra.Quantity)) ?? 0;
        var context = new DiscardSpecs.DiscardHalfContext(GamePhase, req, discards, discardTotal, player);

        foreach (var spec in DiscardHalfSpecifications)
        {
            var result = spec.IsSatisfiedBy(context);
            if (result.IsFailure) return result;
        }

        foreach (var (type, amount) in discards!)
        {
            player!.SubtractResource(type, amount);
            BankResourceHand.Add(type, amount);
        }

        _discardHalfRequirements.Remove(req!);
        AddDomainEvent(new PlayerDiscardedHalfDomainEvent(Id, playerId, discards));

        if (_discardHalfRequirements.Count == 0)
        {
            GamePhase = GamePhase.ResolveRobber;
            AddDomainEvent(new DiscardHalfCompleteDomainEvent(Id));
        }

        return Result.Success();
    }
}
