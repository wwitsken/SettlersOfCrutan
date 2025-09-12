using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.DomainErrors;
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
            return Result.Failure<(Road, Road)>(DomainError.WrongGamePhase);
        if (CurrentPlayerId() != playerId)
            return Result.Failure<(Road, Road)>(DomainError.WrongTurn);

        var player = Players.First(p => p.Id == playerId);
        if (!player.CanUseDevCard(DevelopmentCardType.RoadBuilding) || !player.CanConsumePiece(BuildableType.Road, 2))
            return Result.Failure<(Road, Road)>(DomainError.CannotPlayRoadBuilding);

        var can1 = Board.CanPlaceRoad(playerId, edge1);
        if (can1.IsFailure) return Result.Failure<(Road, Road)>(can1.Error);
        var can2 = Board.CanPlaceRoad(playerId, edge2);
        if (can2.IsFailure) return Result.Failure<(Road, Road)>(can2.Error);

        if (edge1.HexCoords().Intersect(edge2.HexCoords()).Count() != 1)
        {
            // they must both connect to the existing network; already validated individually
        }

        var r1 = Board.PlaceRoadNoFail(playerId, edge1);
        var r2 = Board.PlaceRoadNoFail(playerId, edge2);

        player.UseDevCardToBank(BankDevCardHand, DevelopmentCardType.RoadBuilding);
        player.ConsumePiece(BuildableType.Road, 2);

        AddDomainEvent(new RoadBuildingCardPlayedDomainEvent(Id, playerId, r1, r2));
        return Result.Success((r1, r2));
    }

    public Result<int> PlayMonopoly(PlayerId playerId, ResourceCardType resourceType)
    {
        if (GamePhase != GamePhase.TradeBuild) return Result.Failure<int>(DomainError.WrongGamePhase);
        if (CurrentPlayerId() != playerId) return Result.Failure<int>(DomainError.WrongTurn);

        var player = Players.First(p => p.Id == playerId);
        if (!player.CanUseDevCard(DevelopmentCardType.Monopoly))
            return Result.Failure<int>(DomainError.MissingMonopolyCard);

        int totalStolen = 0;
        var stolenPerPlayer = new Dictionary<PlayerId, int>();
        foreach (var other in Players.Where(p => p.Id != playerId))
        {
            var moved = other.MoveAllOfResourceTo(BankResourceHand, resourceType); // temporarily move to bank
            if (moved <= 0) continue;
            BankResourceHand.Transfer(player.ResourceHand, resourceType, moved); // then bank gives to player
            totalStolen += moved;
            stolenPerPlayer[other.Id] = moved;
        }

        player.UseDevCardToBank(BankDevCardHand, DevelopmentCardType.Monopoly);
        AddDomainEvent(new MonopolyCardPlayedDomainEvent(Id, playerId, resourceType, stolenPerPlayer, totalStolen));
        return Result.Success(totalStolen);
    }

    public Result<(ResourceCardType t1, ResourceCardType t2)> PlayYearOfPlenty(PlayerId playerId, ResourceCardType t1, ResourceCardType t2)
    {
        if (GamePhase != GamePhase.TradeBuild) return Result.Failure<(ResourceCardType, ResourceCardType)>(DomainError.WrongGamePhase);
        if (CurrentPlayerId() != playerId) return Result.Failure<(ResourceCardType, ResourceCardType)>(DomainError.WrongTurn);

        var player = Players.First(p => p.Id == playerId);
        if (!player.CanUseDevCard(DevelopmentCardType.YearOfPlenty))
            return Result.Failure<(ResourceCardType, ResourceCardType)>(DomainError.MissingYearOfPlentyCard);

        if (t1 == t2)
        {
            if (!BankResourceHand.HasAtLeast(new ResourceCardAmount(t1, 2)))
                return Result.Failure<(ResourceCardType, ResourceCardType)>(DomainError.InsufficientResources);
        }
        else
        {
            if (!BankResourceHand.HasAtLeast(new ResourceCardAmount(t1, 1)) || !BankResourceHand.HasAtLeast(new ResourceCardAmount(t2, 1)))
                return Result.Failure<(ResourceCardType, ResourceCardType)>(DomainError.InsufficientResources);
        }

        player.AddResource(t1); player.AddResource(t2);
        BankResourceHand.Subtract(t1); BankResourceHand.Subtract(t2);
        player.UseDevCardToBank(BankDevCardHand, DevelopmentCardType.YearOfPlenty);
        AddDomainEvent(new YearOfPlentyCardPlayedDomainEvent(Id, playerId, t1, t2));
        return Result.Success((t1, t2));
    }

    public Result<HexCoord> PlayKnight(PlayerId playerId, HexCoord newRobberHexCoord, PlayerId victimId)
    {
        if (GamePhase != GamePhase.TradeBuild) return Result.Failure<HexCoord>(DomainError.WrongGamePhase);
        if (CurrentPlayerId() != playerId) return Result.Failure<HexCoord>(DomainError.WrongTurn);

        var player = Players.First(p => p.Id == playerId);
        if (!player.CanUseDevCard(DevelopmentCardType.Knight))
            return Result.Failure<HexCoord>(DomainError.MissingKnightCard);

        player.UseDevCardToBank(BankDevCardHand, DevelopmentCardType.Knight);
        GamePhase = GamePhase.ResolveRobber;
        AddDomainEvent(new KnightCardPlayedDomainEvent(Id, playerId));
        AddDomainEvent(new RobPlayerStartedDomainEvent(Id));
        return Result.Success(newRobberHexCoord);
    }
}
