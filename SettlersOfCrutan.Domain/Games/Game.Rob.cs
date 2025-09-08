using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.Games.Boards.Coordinates;
using SettlersOfCrutan.Domain.Games.DomainEvents;

namespace SettlersOfCrutan.Domain.Games;
public partial class Game
{
    public Result<ResourceType> ResolveRobber(PlayerId robbingPlayerId, HexCoord newRobberHexCoord, PlayerId victimId)
    {
        if (CurrentPlayerId() != robbingPlayerId) return Result<ResourceType>.Failure(DomainErrors.DomainError.WrongTurn);

        if (Board.IsPlayerExposedToHex(newRobberHexCoord, victimId) == false)
            return Result<ResourceType>.Failure(new Error("Robber", "Victim player is not exposed to the new robber hex"));

        var moved = Board.MoveRobber(newRobberHexCoord);
        if (moved.IsFailure) return Result.Failure<ResourceType>(moved.Error);

        var rng = new Random();
        var victimResources = Players.First(p => p.Id == victimId).ResourceBag;
        List<ResourceType> victimResourceTypes = [.. Enum.GetValues<ResourceType>().SelectMany(rt => ListResourceType(rt, victimResources))];
        if (victimResourceTypes.Count == 0) return Result.Success(ResourceType.None);

        var stolenResource = victimResourceTypes[rng.Next(victimResourceTypes.Count)];
        victimResources.SubtractResource(stolenResource, 1);
        var playerResources = Players.First(p => p.Id == robbingPlayerId).ResourceBag;
        playerResources.AddResource(stolenResource, 1);
        AddDomainEvent(new RobberResolvedDomainEvent(Id, newRobberHexCoord, stolenResource, robbingPlayerId, victimId));
        return Result.Success(stolenResource);
    }

    private static List<ResourceType> ListResourceType(ResourceType resourceType, ResourceBag resourceBag)
    {
        List<ResourceType> resources = [];
        for (int i = 0; i < resourceBag.Count(resourceType); i++) resources.Add(resourceType);
        return resources;
    }
}
