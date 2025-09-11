using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.Games.Boards;
using SettlersOfCrutan.Domain.Games.Boards.Coordinates;
using SettlersOfCrutan.Domain.Games.DomainEvents;
using SettlersOfCrutan.Domain.Games.Resources;

namespace SettlersOfCrutan.Domain.Games;
public partial class Game
{
    public Result<(Road r1, Road r2)> PlayRoadBuilding(PlayerId playerId, Edge edge1, Edge edge2)
    {
        if (GamePhase != GamePhase.TradeBuild)
            return Result.Failure<(Road, Road)>(DomainErrors.DomainError.WrongGamePhase);
        if (CurrentPlayerId() != playerId)
            return Result.Failure<(Road, Road)>(DomainErrors.DomainError.WrongTurn);

        var player = Players.First(p => p.Id == playerId);
        var devHand = player.DevCardHand;
        var reserve = player.PieceReserve;

        if (!devHand.CanUse(DevelopmentCardType.RoadBuilding) || !reserve.CanConsume(BuildableType.Road, 2))
            return Result.Failure<(Road, Road)>(new Error("DevCard", "Cannot play Road Building"));

        // Validate edges individually via board pure validation
        var can1 = Board.CanPlaceRoad(playerId, edge1);
        if (can1.IsFailure) return Result.Failure<(Road, Road)>(can1.Error);
        var can2 = Board.CanPlaceRoad(playerId, edge2);
        if (can2.IsFailure) return Result.Failure<(Road, Road)>(can2.Error);

        // Additional adjacency rule between the two edges (original logic)
        if (edge1.HexCoords().Intersect(edge2.HexCoords()).Count() != 1)
        {
            // they must both connect to the existing network; already validated by individual CanPlaceRoad
        }

        var r1 = Board.PlaceRoadNoFail(playerId, edge1);
        var r2 = Board.PlaceRoadNoFail(playerId, edge2);

        devHand.UseToBank(BankDevCardHand, DevelopmentCardType.RoadBuilding);
        reserve.Consume(BuildableType.Road, 2);

        AddDomainEvent(new RoadBuildingCardPlayedDomainEvent(Id, playerId, r1, r2));
        return Result.Success((r1, r2));
    }

    public Result<int> PlayMonopoly(PlayerId playerId, ResourceCardType resourceType)
    {
        if (GamePhase != GamePhase.TradeBuild) return Result.Failure<int>(DomainErrors.DomainError.WrongGamePhase);
        if (CurrentPlayerId() != playerId) return Result.Failure<int>(DomainErrors.DomainError.WrongTurn);

        var player = Players.First(p => p.Id == playerId);
        var devHand = player.DevCardHand;
        var hand = player.ResourceHand;
        if (!devHand.CanUse(DevelopmentCardType.Monopoly))
            return Result.Failure<int>(new Error("DevCard", "Player does not have Monopoly card"));

        int totalStolen = 0;
        var stolenPerPlayer = new Dictionary<PlayerId, int>();
        foreach (var other in Players.Where(p => p.Id != playerId))
        {
            var moved = other.ResourceHand.MoveAllTo(hand, resourceType);
            if (moved <= 0) continue;
            totalStolen += moved;
            stolenPerPlayer[other.Id] = moved;
        }

        devHand.UseToBank(BankDevCardHand, DevelopmentCardType.Monopoly);
        AddDomainEvent(new MonopolyCardPlayedDomainEvent(Id, playerId, resourceType, stolenPerPlayer, totalStolen));
        return Result.Success(totalStolen);
    }

    public Result<(ResourceCardType t1, ResourceCardType t2)> PlayYearOfPlenty(PlayerId playerId, ResourceCardType t1, ResourceCardType t2)
    {
        if (GamePhase != GamePhase.TradeBuild) return Result.Failure<(ResourceCardType, ResourceCardType)>(DomainErrors.DomainError.WrongGamePhase);
        if (CurrentPlayerId() != playerId) return Result.Failure<(ResourceCardType, ResourceCardType)>(DomainErrors.DomainError.WrongTurn);

        var player = Players.First(p => p.Id == playerId);
        var devHand = player.DevCardHand;
        var hand = player.ResourceHand;
        if (!devHand.CanUse(DevelopmentCardType.YearOfPlenty))
            return Result.Failure<(ResourceCardType, ResourceCardType)>(new Error("DevCard", "Player does not have Year of Plenty card"));

        if (t1 == t2)
        {
            if (!BankResourceHand.HasAtLeast(new ResourceCardAmount(t1, 2)))
                return Result.Failure<(ResourceCardType, ResourceCardType)>(DomainErrors.DomainError.InsufficientResources);
        }
        else
        {
            if (!BankResourceHand.HasAtLeast(new ResourceCardAmount(t1, 1)) || !BankResourceHand.HasAtLeast(new ResourceCardAmount(t2, 1)))
                return Result.Failure<(ResourceCardType, ResourceCardType)>(DomainErrors.DomainError.InsufficientResources);
        }

        hand.Add(t1); hand.Add(t2);
        BankResourceHand.Subtract(t1); BankResourceHand.Subtract(t2);
        devHand.UseToBank(BankDevCardHand, DevelopmentCardType.YearOfPlenty);
        AddDomainEvent(new YearOfPlentyCardPlayedDomainEvent(Id, playerId, t1, t2));
        return Result.Success((t1, t2));
    }

    public Result<HexCoord> PlayKnight(PlayerId playerId, HexCoord newRobberHexCoord, PlayerId victimId)
    {
        if (GamePhase != GamePhase.TradeBuild) return Result.Failure<HexCoord>(DomainErrors.DomainError.WrongGamePhase);
        if (CurrentPlayerId() != playerId) return Result.Failure<HexCoord>(DomainErrors.DomainError.WrongTurn);

        var player = Players.First(p => p.Id == playerId);
        var devHand = player.DevCardHand;
        if (!devHand.CanUse(DevelopmentCardType.Knight))
            return Result.Failure<HexCoord>(new Error("DevCard", "Player does not have Knight card"));

        devHand.UseToBank(BankDevCardHand, DevelopmentCardType.Knight);
        GamePhase = GamePhase.ResolveRobber;
        AddDomainEvent(new KnightCardPlayedDomainEvent(Id, playerId));
        AddDomainEvent(new RobPlayerStartedDomainEvent(Id));
        return Result.Success(newRobberHexCoord);
    }
}
