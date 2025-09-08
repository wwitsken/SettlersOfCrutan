using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.Games.Boards;
using SettlersOfCrutan.Domain.Games.Boards.Coordinates;
using SettlersOfCrutan.Domain.Games.DomainEvents;

namespace SettlersOfCrutan.Domain.Games;
public partial class Game
{
    public Result<(Road r1, Road r2)> PlayRoadBuilding(PlayerId playerId, Edge edge1, Edge edge2)
    {
        if (CurrentPlayerId() != playerId)
            return Result.Failure<(Road, Road)>(DomainErrors.DomainError.WrongTurn);
        if (GamePhase != GamePhase.TradeBuild)
            return Result.Failure<(Road, Road)>(DomainErrors.DomainError.WrongGamePhase);

        var player = Players.First(p => p.Id == playerId);

        // Check that resources are available
        var bag = player.ResourceBag;
        if (!bag.HasDevelopmentCard(DevelopmentCardType.RoadBuilding))
            return Result.Failure<(Road, Road)>(new Error("DevCard", "Player does not have Road Building card"));
        if (bag.Roads < 2)
            return Result.Failure<(Road, Road)>(DomainErrors.DomainError.MissingRoad);

        // if 1 and 2 don't connect, then they both need to connect to existing roads
        // if 1 and 2 do connect, then only one of them needs to connect to existing roads
        if (edge1.HexCoords().Intersect(edge2.HexCoords()).Count() == 1)
        {
            // They connect - only one of them needs to connect to existing network
            if (!Board.IsEdgeBuildableBy(CurrentPlayerId(), edge1) && !Board.IsEdgeBuildableBy(CurrentPlayerId(), edge2))
                // Neither of them connect to existing network
                return Result.Failure<(Road, Road)>(new Error("RoadBuild", "Neither road connects to existing road"));
        }
        else
        {
            // They don't connect to eachother - they must both connect to the existing network
            if (!Board.IsEdgeBuildableBy(CurrentPlayerId(), edge1))
                return Result.Failure<(Road, Road)>(new Error("RoadBuild", "First road does not connect to existing road"));
            if (!Board.IsEdgeBuildableBy(CurrentPlayerId(), edge2))
                return Result.Failure<(Road, Road)>(new Error("RoadBuild", "Second road does not connect to existing road"));
        }

        Result<Road> r1 = Board.BuildRoad(playerId, edge1);
        Result<Road> r2 = Board.BuildRoad(playerId, edge2);
        if (r1.IsFailure && r2.IsSuccess)
            r1 = Board.BuildRoad(playerId, edge1);

        // Final check - make sure that if either fail, we don't continue
        if (r1.IsFailure || r2.IsFailure)
            return Result.Failure<(Road, Road)>(new Error("RoadBuild", $"Failed to build roads: {r1.Error.Message}; {r2.Error.Message}"));

        bag.RemoveSingleDevelopmentCard(DevelopmentCardType.RoadBuilding);
        Bank.AddSingleDevelopmentCard(DevelopmentCardType.RoadBuilding);
        bag.Roads -= 2;

        AddDomainEvent(new RoadBuildingCardPlayedDomainEvent(Id, playerId, r1.Value, r2.Value));
        return Result.Success((r1.Value, r2.Value));
    }

    public Result<int> PlayMonopoly(PlayerId playerId, ResourceType resourceType)
    {
        if (CurrentPlayerId() != playerId) return Result.Failure<int>(DomainErrors.DomainError.WrongTurn);
        if (GamePhase != GamePhase.TradeBuild) return Result.Failure<int>(DomainErrors.DomainError.WrongGamePhase);

        var player = Players.First(p => p.Id == playerId);
        var bag = player.ResourceBag;
        if (!bag.HasDevelopmentCard(DevelopmentCardType.Monopoly))
            return Result.Failure<int>(new Error("DevCard", "Player does not have Monopoly card"));

        int totalStolen = 0;
        var stolenPerPlayer = new Dictionary<PlayerId, int>();

        foreach (var other in Players.Where(p => p.Id != playerId))
        {
            var count = other.ResourceBag.Count(resourceType);
            if (count <= 0) continue;
            other.ResourceBag.SubtractResource(resourceType, count);
            bag.AddResource(resourceType, count);
            totalStolen += count;
            stolenPerPlayer[other.Id] = count;
        }

        // Consume the card
        bag.RemoveSingleDevelopmentCard(DevelopmentCardType.Monopoly);
        Bank.AddSingleDevelopmentCard(DevelopmentCardType.Monopoly);

        AddDomainEvent(new MonopolyCardPlayedDomainEvent(Id, playerId, resourceType, stolenPerPlayer, totalStolen));
        return Result.Success(totalStolen);
    }

    public Result<(ResourceType t1, ResourceType t2)> PlayYearOfPlenty(PlayerId playerId, ResourceType t1, ResourceType t2)
    {
        if (CurrentPlayerId() != playerId) return Result.Failure<(ResourceType, ResourceType)>(DomainErrors.DomainError.WrongTurn);
        if (GamePhase != GamePhase.TradeBuild) return Result.Failure<(ResourceType, ResourceType)>(DomainErrors.DomainError.WrongGamePhase);

        var player = Players.First(p => p.Id == playerId);
        var bag = player.ResourceBag;
        if (!bag.HasDevelopmentCard(DevelopmentCardType.YearOfPlenty))
            return Result.Failure<(ResourceType, ResourceType)>(new Error("DevCard", "Player does not have Year of Plenty card"));

        // Check bank availability
        var need1 = new ResourceAmount(t1, 1);
        var need2 = new ResourceAmount(t2, 1);
        if (t1 == t2)
        {
            if (!Bank.HasAtLeast(new ResourceAmount(t1, 2)))
                return Result.Failure<(ResourceType, ResourceType)>(DomainErrors.DomainError.InsufficientResources);
        }
        else
        {
            if (!Bank.HasAtLeast(need1) || !Bank.HasAtLeast(need2))
                return Result.Failure<(ResourceType, ResourceType)>(DomainErrors.DomainError.InsufficientResources);
        }

        // Apply transfers
        bag.ApplyResourceAmounts([need1, need2]);
        Bank.ApplyResourceAmounts([need1.Invert(), need2.Invert()]);

        // Consume the card
        bag.RemoveSingleDevelopmentCard(DevelopmentCardType.YearOfPlenty);
        Bank.AddSingleDevelopmentCard(DevelopmentCardType.YearOfPlenty);

        AddDomainEvent(new YearOfPlentyCardPlayedDomainEvent(Id, playerId, t1, t2));
        return Result.Success((t1, t2));
    }

    public Result<HexCoord> PlayKnight(PlayerId playerId, HexCoord newRobberHexCoord, PlayerId victimId)
    {
        if (CurrentPlayerId() != playerId) return Result.Failure<HexCoord>(DomainErrors.DomainError.WrongTurn);
        if (GamePhase != GamePhase.TradeBuild) return Result.Failure<HexCoord>(DomainErrors.DomainError.WrongGamePhase);

        var player = Players.First(p => p.Id == playerId);
        var bag = player.ResourceBag;
        if (!bag.HasDevelopmentCard(DevelopmentCardType.Knight))
            return Result.Failure<HexCoord>(new Error("DevCard", "Player does not have Knight card"));

        // Consume the card
        bag.RemoveSingleDevelopmentCard(DevelopmentCardType.Knight);
        Bank.AddSingleDevelopmentCard(DevelopmentCardType.Knight);

        // Enter robber resolution flow
        GamePhase = GamePhase.ResolveRobber;
        AddDomainEvent(new KnightCardPlayedDomainEvent(Id, playerId));
        AddDomainEvent(new RobPlayerStartedDomainEvent(Id));

        // The actual robber move/steal should be handled by ResolveRobber afterward
        return Result.Success(newRobberHexCoord);
    }
}
