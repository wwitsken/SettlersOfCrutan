using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.Games.Boards;
using SettlersOfCrutan.Domain.Games.Boards.Coordinates;
using SettlersOfCrutan.Domain.Games.DomainEvents;
using SettlersOfCrutan.Domain.Games.Resources;

namespace SettlersOfCrutan.Domain.Games;
public partial class Game
{
    public Result<(PopulationCenter, Road)> PlaceInitial(PlayerId playerId, Vertex settlementVertex, Edge roadEdge, IDateTimeProvider clock)
    {
        if (CurrentPlayerId() != playerId) return Result.Failure<(PopulationCenter, Road)>(DomainErrors.DomainError.WrongTurn);
        if (GamePhase != GamePhase.Setup) return Result.Failure<(PopulationCenter, Road)>(DomainErrors.DomainError.WrongGamePhase);

        var reserve = Players.First(p => p.Id == playerId).PieceReserve;
        if (!reserve.CanConsume(BuildableType.Settlement) || !reserve.CanConsume(BuildableType.Road))
            return Result.Failure<(PopulationCenter, Road)>(new Error("InitialPieces", "Missing initial pieces"));

        var can = Board.CanPlaceInitialSettleAndRoad(settlementVertex, roadEdge);
        if (can.IsFailure) return Result.Failure<(PopulationCenter, Road)>(can.Error);

        var (settlement, road) = Board.PlaceInitialSettleAndRoadNoFail(playerId, settlementVertex, roadEdge);
        reserve.Consume(BuildableType.Settlement);
        reserve.Consume(BuildableType.Road);

        EndTurn(playerId, clock);
        AddDomainEvent(new InitialSettlementAndRoadPlacedDomainEvent(Id, playerId, settlement, road));
        return Result.Success((settlement, road));
    }

    public Result<Road> BuildRoad(IPriceCalculator priceCalculator, PlayerId playerId, Edge edge)
    {
        if (CurrentPlayerId() != playerId) return Result.Failure<Road>(DomainErrors.DomainError.WrongTurn);
        if (GamePhase != GamePhase.TradeBuild) return Result.Failure<Road>(DomainErrors.DomainError.WrongGamePhase);

        var player = Players.First(p => p.Id == playerId);
        var reserve = player.PieceReserve;
        var hand = player.ResourceHand;
        var cost = priceCalculator.RoadPrice();

        if (!hand.CanPay(cost)) return Result.Failure<Road>(DomainErrors.DomainError.InsufficientResources);
        if (!reserve.CanConsume(BuildableType.Road)) return Result.Failure<Road>(DomainErrors.DomainError.MissingRoad);
        var canPlace = Board.CanPlaceRoad(playerId, edge);
        if (canPlace.IsFailure) return Result.Failure<Road>(canPlace.Error);

        var road = Board.PlaceRoadNoFail(playerId, edge);
        hand.PayTo(BankResourceHand, cost);
        reserve.Consume(BuildableType.Road);

        AddDomainEvent(new RoadBuiltDomainEvent(Id, playerId, road));
        return Result.Success(road);
    }

    public Result<PopulationCenter> BuildSettlement(IPriceCalculator priceCalculator, PlayerId playerId, Vertex vertex)
    {
        if (CurrentPlayerId() != playerId) return Result.Failure<PopulationCenter>(DomainErrors.DomainError.WrongTurn);
        if (GamePhase != GamePhase.TradeBuild) return Result.Failure<PopulationCenter>(DomainErrors.DomainError.WrongGamePhase);

        var player = Players.First(p => p.Id == playerId);
        var reserve = player.PieceReserve;
        var hand = player.ResourceHand;
        var cost = priceCalculator.SettlementPrice();

        if (!hand.CanPay(cost)) return Result.Failure<PopulationCenter>(DomainErrors.DomainError.InsufficientResources);
        if (!reserve.CanConsume(BuildableType.Settlement)) return Result.Failure<PopulationCenter>(DomainErrors.DomainError.MissingSettlement);
        var can = Board.CanPlaceSettlement(playerId, vertex);
        if (can.IsFailure) return Result.Failure<PopulationCenter>(can.Error);

        var settlement = Board.PlaceSettlementNoFail(playerId, vertex);
        hand.PayTo(BankResourceHand, cost);
        reserve.Consume(BuildableType.Settlement);

        AddDomainEvent(new SettlementBuiltDomainEvent(Id, playerId, settlement));
        return Result.Success(settlement);
    }

    public Result<PopulationCenter> BuildCity(IPriceCalculator priceCalculator, PlayerId playerId, Vertex vertex)
    {
        if (CurrentPlayerId() != playerId) return Result.Failure<PopulationCenter>(DomainErrors.DomainError.WrongTurn);
        if (GamePhase != GamePhase.TradeBuild) return Result.Failure<PopulationCenter>(DomainErrors.DomainError.WrongGamePhase);

        var player = Players.First(p => p.Id == playerId);
        var reserve = player.PieceReserve;
        var hand = player.ResourceHand;
        var cost = priceCalculator.CityPrice();

        if (!hand.CanPay(cost)) return Result.Failure<PopulationCenter>(DomainErrors.DomainError.InsufficientResources);
        if (!reserve.CanConsume(BuildableType.City)) return Result.Failure<PopulationCenter>(DomainErrors.DomainError.MissingCity);
        var can = Board.CanUpgradeToCity(playerId, vertex);
        if (can.IsFailure) return Result.Failure<PopulationCenter>(can.Error);

        var city = Board.UpgradeToCityNoFail(playerId, vertex);
        hand.PayTo(BankResourceHand, cost);
        reserve.Consume(BuildableType.City);
        reserve.Add(BuildableType.Settlement); // return settlement piece

        AddDomainEvent(new SettlementUpgradedToCityDomainEvent(Id, playerId, city));
        return Result.Success(city);
    }

    public Result<DevelopmentCardType> BuyDevelopmentCard(IPriceCalculator priceCalculator, PlayerId playerId)
    {
        if (CurrentPlayerId() != playerId) return Result.Failure<DevelopmentCardType>(DomainErrors.DomainError.WrongTurn);
        if (GamePhase != GamePhase.TradeBuild) return Result.Failure<DevelopmentCardType>(DomainErrors.DomainError.WrongGamePhase);

        var player = Players.First(p => p.Id == playerId);
        var hand = player.ResourceHand;
        var devHand = player.DevCardHand;
        var cost = priceCalculator.DevelopmentCardPrice();

        if (!hand.CanPay(cost)) return Result.Failure<DevelopmentCardType>(DomainErrors.DomainError.InsufficientResources);
        if (devHand.Cards.Count > 3) return Result.Failure<DevelopmentCardType>(DomainErrors.DomainError.TooManyDevelopmentCards);
        if (BankDevCardHand.Cards.Count <= 0) return Result.Failure<DevelopmentCardType>(DomainErrors.DomainError.InsufficientBankDevelopmentCards);

        var drawn = BankDevCardHand.DrawRandom();
        devHand.Add(drawn);
        hand.PayTo(BankResourceHand, cost);
        AddDomainEvent(new DevelopmentCardPurchasedDomainEvent(Id, playerId, drawn));
        return Result.Success(drawn);
    }
}
