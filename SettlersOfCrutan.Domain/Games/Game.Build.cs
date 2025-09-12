using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.DomainErrors;
using SettlersOfCrutan.Domain.Games.Boards;
using SettlersOfCrutan.Domain.Games.Boards.Coordinates;
using SettlersOfCrutan.Domain.Games.DomainEvents;
using SettlersOfCrutan.Domain.Games.Resources;

namespace SettlersOfCrutan.Domain.Games;
public partial class Game
{
    public Result<(PopulationCenter, Road)> PlaceInitial(PlayerId playerId, Vertex settlementVertex, Edge roadEdge, IDateTimeProvider clock)
    {
        if (GamePhase != GamePhase.Setup) return Result.Failure<(PopulationCenter, Road)>(DomainError.WrongGamePhase);
        if (CurrentPlayerId() != playerId) return Result.Failure<(PopulationCenter, Road)>(DomainError.WrongTurn);

        var player = Players.First(p => p.Id == playerId);
        if (!player.CanConsumePiece(BuildableType.Settlement) || !player.CanConsumePiece(BuildableType.Road))
            return Result.Failure<(PopulationCenter, Road)>(DomainError.MissingInitialPieces);

        var can = Board.CanPlaceInitialSettleAndRoad(settlementVertex, roadEdge);
        if (can.IsFailure) return Result.Failure<(PopulationCenter, Road)>(can.Error);

        var (settlement, road) = Board.PlaceInitialSettleAndRoadNoFail(playerId, settlementVertex, roadEdge);
        player.ConsumePiece(BuildableType.Settlement);
        player.ConsumePiece(BuildableType.Road);

        EndTurn(playerId, clock);
        AddDomainEvent(new InitialSettlementAndRoadPlacedDomainEvent(Id, playerId, settlement, road));
        return Result.Success((settlement, road));
    }

    public Result<Road> BuildRoad(IPriceCalculator priceCalculator, PlayerId playerId, Edge edge)
    {
        if (GamePhase != GamePhase.TradeBuild) return Result.Failure<Road>(DomainError.WrongGamePhase);
        if (CurrentPlayerId() != playerId) return Result.Failure<Road>(DomainError.WrongTurn);

        var player = Players.First(p => p.Id == playerId);
        var cost = priceCalculator.RoadPrice();

        if (!player.CanPayResources(cost)) return Result.Failure<Road>(DomainError.InsufficientResources);
        if (!player.CanConsumePiece(BuildableType.Road)) return Result.Failure<Road>(DomainError.MissingRoad);
        var canPlace = Board.CanPlaceRoad(playerId, edge);
        if (canPlace.IsFailure) return Result.Failure<Road>(canPlace.Error);

        var road = Board.PlaceRoadNoFail(playerId, edge);
        player.PayResourcesTo(BankResourceHand, cost);
        player.ConsumePiece(BuildableType.Road);

        AddDomainEvent(new RoadBuiltDomainEvent(Id, playerId, road));
        return Result.Success(road);
    }

    public Result<PopulationCenter> BuildSettlement(IPriceCalculator priceCalculator, PlayerId playerId, Vertex vertex)
    {
        if (GamePhase != GamePhase.TradeBuild) return Result.Failure<PopulationCenter>(DomainError.WrongGamePhase);
        if (CurrentPlayerId() != playerId) return Result.Failure<PopulationCenter>(DomainError.WrongTurn);

        var player = Players.First(p => p.Id == playerId);
        var cost = priceCalculator.SettlementPrice();

        if (!player.CanPayResources(cost)) return Result.Failure<PopulationCenter>(DomainError.InsufficientResources);
        if (!player.CanConsumePiece(BuildableType.Settlement)) return Result.Failure<PopulationCenter>(DomainError.MissingSettlement);
        var can = Board.CanPlaceSettlement(playerId, vertex);
        if (can.IsFailure) return Result.Failure<PopulationCenter>(can.Error);

        var settlement = Board.PlaceSettlementNoFail(playerId, vertex);
        player.PayResourcesTo(BankResourceHand, cost);
        player.ConsumePiece(BuildableType.Settlement);

        AddDomainEvent(new SettlementBuiltDomainEvent(Id, playerId, settlement));
        return Result.Success(settlement);
    }

    public Result<PopulationCenter> BuildCity(IPriceCalculator priceCalculator, PlayerId playerId, Vertex vertex)
    {
        if (GamePhase != GamePhase.TradeBuild) return Result.Failure<PopulationCenter>(DomainError.WrongGamePhase);
        if (CurrentPlayerId() != playerId) return Result.Failure<PopulationCenter>(DomainError.WrongTurn);

        var player = Players.First(p => p.Id == playerId);
        var cost = priceCalculator.CityPrice();

        if (!player.CanPayResources(cost)) return Result.Failure<PopulationCenter>(DomainError.InsufficientResources);
        if (!player.CanConsumePiece(BuildableType.City)) return Result.Failure<PopulationCenter>(DomainError.MissingCity);
        var can = Board.CanUpgradeToCity(playerId, vertex);
        if (can.IsFailure) return Result.Failure<PopulationCenter>(can.Error);

        var city = Board.UpgradeToCityNoFail(playerId, vertex);
        player.PayResourcesTo(BankResourceHand, cost);
        player.ConsumePiece(BuildableType.City);
        player.ReturnPiece(BuildableType.Settlement); // return settlement piece

        AddDomainEvent(new SettlementUpgradedToCityDomainEvent(Id, playerId, city));
        return Result.Success(city);
    }

    public Result<DevelopmentCardType> BuyDevelopmentCard(IPriceCalculator priceCalculator, PlayerId playerId)
    {
        if (GamePhase != GamePhase.TradeBuild) return Result.Failure<DevelopmentCardType>(DomainError.WrongGamePhase);
        if (CurrentPlayerId() != playerId) return Result.Failure<DevelopmentCardType>(DomainError.WrongTurn);

        var player = Players.First(p => p.Id == playerId);
        var cost = priceCalculator.DevelopmentCardPrice();

        if (!player.CanPayResources(cost)) return Result.Failure<DevelopmentCardType>(DomainError.InsufficientResources);
        if (player.DevCardHand.Cards.Count > 3) return Result.Failure<DevelopmentCardType>(DomainError.TooManyDevelopmentCards);
        if (BankDevCardHand.Cards.Count <= 0) return Result.Failure<DevelopmentCardType>(DomainError.InsufficientBankDevelopmentCards);

        var drawn = BankDevCardHand.DrawRandom();
        player.AddDevCard(drawn);
        player.PayResourcesTo(BankResourceHand, cost);

        AddDomainEvent(new DevelopmentCardPurchasedDomainEvent(Id, playerId, drawn));
        return Result.Success(drawn);
    }
}
