using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.Games.Boards.Coordinates;
using SettlersOfCrutan.Domain.Games.DomainEvents;
using SettlersOfCrutan.Domain.Games.Resources;
using SettlersOfCrutan.Domain.Specifications;
using RobSpecs = SettlersOfCrutan.Domain.Specifications.ResolveRobber;

namespace SettlersOfCrutan.Domain.Games;
public partial class Game
{
    private static readonly ISpecification<RobSpecs.ResolveRobberContext>[] ResolveRobberSpecifications =
    [
        new RobSpecs.GameMustBeInResolveRobberPhase(),
        new RobSpecs.MustBeCurrentPlayerTurn(),
        new RobSpecs.RobberDestinationMustBeValid(),
        new RobSpecs.RobberVictimMustBeValid()
    ];

    /// <param name="victimId">Required when at least one opponent has a settlement/city on the hex; otherwise omit (no steal).</param>
    public Result<ResourceCardType> ResolveRobber(PlayerId robbingPlayerId, HexCoord newRobberHexCoord, PlayerId? victimId)
    {
        var stealTargets = Board.GetOpponentIdsOnHex(newRobberHexCoord, robbingPlayerId);
        var context = new RobSpecs.ResolveRobberContext(
            GamePhase, CurrentPlayerId(), robbingPlayerId, Board, newRobberHexCoord, victimId, stealTargets);

        foreach (var spec in ResolveRobberSpecifications)
        {
            var result = spec.IsSatisfiedBy(context);
            if (result.IsFailure)
                return Result.Failure<ResourceCardType>(result.Error);
        }

        var stolen = ResolveRobberNoFail(robbingPlayerId, newRobberHexCoord, victimId);
        return Result.Success(stolen);
    }

    private static List<ResourceCardType> ListResourceType(ResourceCardType resourceType, Player victim)
    {
        List<ResourceCardType> resources = [];
        for (int i = 0; i < victim.CountResource(resourceType); i++) resources.Add(resourceType);
        return resources;
    }

    private ResourceCardType ResolveRobberNoFail(PlayerId robbingPlayerId, HexCoord newRobberHexCoord, PlayerId? victimId)
    {
        var moved = Board.MoveRobber(newRobberHexCoord);
        if (moved.IsFailure) throw new InvalidOperationException("Precondition failed: robber move invalid");

        ResourceCardType stolenResource = ResourceCardType.None;
        if (victimId is not null)
        {
            var rng = new Random();
            var victim = Players.First(p => p.Id == victimId);
            List<ResourceCardType> victimResourceTypes = [.. Enum.GetValues<ResourceCardType>().SelectMany(rt => ListResourceType(rt, victim))];

            stolenResource = victimResourceTypes.Count == 0
                ? ResourceCardType.None
                : victimResourceTypes[rng.Next(victimResourceTypes.Count)];

            if (stolenResource != ResourceCardType.None)
            {
                victim.SubtractResource(stolenResource, 1);
                var player = Players.First(p => p.Id == robbingPlayerId);
                player.AddResource(stolenResource, 1);
            }
        }

        GamePhase = GamePhase.TradeBuild;
        AddDomainEvent(new RobberResolvedDomainEvent(Id, newRobberHexCoord, stolenResource, robbingPlayerId, victimId));
        AddDomainEvent(new TradeBuildStartedDomainEvent(Id, robbingPlayerId));
        return stolenResource;
    }
}
