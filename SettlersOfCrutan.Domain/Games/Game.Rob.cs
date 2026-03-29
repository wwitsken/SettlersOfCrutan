using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.DomainErrors;
using SettlersOfCrutan.Domain.Games.Boards.Coordinates;
using SettlersOfCrutan.Domain.Games.DomainEvents;
using SettlersOfCrutan.Domain.Games.Resources;

namespace SettlersOfCrutan.Domain.Games;
public partial class Game
{
    /// <param name="victimId">Required when at least one opponent has a settlement/city on the hex; otherwise omit (no steal).</param>
    public Result<ResourceCardType> ResolveRobber(PlayerId robbingPlayerId, HexCoord newRobberHexCoord, PlayerId? victimId)
    {
        var can = CanResolveRobber(robbingPlayerId, newRobberHexCoord, victimId);
        if (can.IsFailure) return Result.Failure<ResourceCardType>(can.Error);
        var stolen = ResolveRobberNoFail(robbingPlayerId, newRobberHexCoord, victimId);
        return Result.Success(stolen);
    }

    private static List<ResourceCardType> ListResourceType(ResourceCardType resourceType, Player victim)
    {
        List<ResourceCardType> resources = [];
        for (int i = 0; i < victim.CountResource(resourceType); i++) resources.Add(resourceType);
        return resources;
    }

    private Result<Nothing> CanResolveRobber(PlayerId robbingPlayerId, HexCoord newRobberHexCoord, PlayerId? victimId)
    {
        if (GamePhase != GamePhase.ResolveRobber)
            return Result.Failure<Nothing>(DomainError.CannotResolveRobberInCurrentPhase);
        if (CurrentPlayerId() != robbingPlayerId)
            return Result.Failure<Nothing>(DomainError.WrongTurn);
        if (!Board.CanMoveRobberTo(newRobberHexCoord))
            return Result.Failure<Nothing>(DomainError.InvalidRobberMove);

        var stealTargets = Board.GetOpponentIdsOnHex(newRobberHexCoord, robbingPlayerId);
        if (stealTargets.Count == 0)
        {
            if (victimId is not null)
                return Result.Failure<Nothing>(DomainError.RobberVictimNotAllowed);
            return Result.Success();
        }

        if (victimId is null)
            return Result.Failure<Nothing>(DomainError.RobberVictimRequired);
        if (!stealTargets.Any(t => t.Equals(victimId)))
            return Result.Failure<Nothing>(DomainError.RobberVictimNotEligible);

        return Result.Success();
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
