using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.Games.Boards.Coordinates;
using SettlersOfCrutan.Domain.Games.DomainEvents;
using SettlersOfCrutan.Domain.Games.Resources;

namespace SettlersOfCrutan.Domain.Games;
public partial class Game
{
    public Result<ResourceCardType> ResolveRobber(PlayerId robbingPlayerId, HexCoord newRobberHexCoord, PlayerId victimId)
    {
        var can = CanResolveRobber(robbingPlayerId, newRobberHexCoord, victimId);
        if (can.IsFailure) return Result.Failure<ResourceCardType>(can.Error);
        var stolen = ResolveRobberNoFail(robbingPlayerId, newRobberHexCoord, victimId);
        return Result.Success(stolen);
    }

    private static List<ResourceCardType> ListResourceType(ResourceCardType resourceType, ResourceHand hand)
    {
        List<ResourceCardType> resources = [];
        for (int i = 0; i < hand.Count(resourceType); i++) resources.Add(resourceType);
        return resources;
    }

    private Result<Nothing> CanResolveRobber(PlayerId robbingPlayerId, HexCoord newRobberHexCoord, PlayerId victimId)
    {
        if (GamePhase != GamePhase.ResolveRobber)
            return Result.Failure<Nothing>(new Error("Robber", "Cannot resolve robber in the current game phase"));
        if (CurrentPlayerId() != robbingPlayerId)
            return Result.Failure<Nothing>(DomainErrors.DomainError.WrongTurn);
        if (!Board.CanMoveRobberTo(newRobberHexCoord))
            return Result.Failure<Nothing>(new Error("Robber", "Invalid robber move"));
        if (!Board.IsPlayerExposedToHex(newRobberHexCoord, victimId))
            return Result.Failure<Nothing>(new Error("Robber", "Victim player is not exposed to the new robber hex"));
        return Result.Success();
    }

    private ResourceCardType ResolveRobberNoFail(PlayerId robbingPlayerId, HexCoord newRobberHexCoord, PlayerId victimId)
    {
        var moved = Board.MoveRobber(newRobberHexCoord);
        if (moved.IsFailure) throw new InvalidOperationException("Precondition failed: robber move invalid");

        var rng = new Random();
        var victimHand = Players.First(p => p.Id == victimId).ResourceHand;
        List<ResourceCardType> victimResourceTypes = [.. Enum.GetValues<ResourceCardType>().SelectMany(rt => ListResourceType(rt, victimHand))];
        if (victimResourceTypes.Count == 0) return ResourceCardType.None;

        var stolenResource = victimResourceTypes[rng.Next(victimResourceTypes.Count)];
        victimHand.Subtract(stolenResource, 1);
        var playerHand = Players.First(p => p.Id == robbingPlayerId).ResourceHand;
        playerHand.Add(stolenResource, 1);
        AddDomainEvent(new RobberResolvedDomainEvent(Id, newRobberHexCoord, stolenResource, robbingPlayerId, victimId));
        return stolenResource;
    }
}
