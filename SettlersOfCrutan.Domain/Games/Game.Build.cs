using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.Games.Boards;
using SettlersOfCrutan.Domain.Games.Boards.Coordinates;
using SettlersOfCrutan.Domain.Games.DomainEvents;

namespace SettlersOfCrutan.Domain.Games;
public partial class Game
{
    public Result<(PopulationCenter, Road)> PlaceInitial(PlayerId playerId, Vertex settlementVertex, Edge roadEdge)
    {
        if (CurrentPlayerId() != playerId) return Result.Failure<(PopulationCenter, Road)>(DomainErrors.DomainError.WrongTurn);
        if (GamePhase != GamePhase.Setup) return Result.Failure<(PopulationCenter, Road)>(DomainErrors.DomainError.WrongGamePhase);

        var playerBag = Players.First(p => p.Id == playerId).ResourceBag;
        if (playerBag.Roads <= 0) return Result.Failure<(PopulationCenter, Road)>(DomainErrors.DomainError.MissingRoad);
        if (playerBag.Settlements <= 0) return Result.Failure<(PopulationCenter, Road)>(DomainErrors.DomainError.MissingSettlement);

        var result = Board.PlaceInitialSettlementAndRoad(playerId, settlementVertex, roadEdge);
        if (result.IsFailure) return Result.Failure<(PopulationCenter, Road)>(result.Error);

        playerBag.Roads -= 1;
        playerBag.Settlements -= 1;

        AddDomainEvent(new InitialSettlementAndRoadPlacedDomainEvent(Id, playerId, result.Value.Item1, result.Value.Item2));

        return result;
    }

    public Result<Road> BuildRoad(PlayerId playerId, Edge edge)
    {
        if (CurrentPlayerId() != playerId) return Result.Failure<Road>(DomainErrors.DomainError.WrongTurn);
        if (GamePhase != GamePhase.TradeBuild) return Result.Failure<Road>(DomainErrors.DomainError.WrongGamePhase);

        var playerBag = Players.First(p => p.Id == playerId).ResourceBag;

        List<ResourceAmount> roadCost = PriceCalculator.RoadPrice();
        if (!playerBag.HasAtLeast(roadCost)) return Result.Failure<Road>(DomainErrors.DomainError.InsufficientResources);
        if (playerBag.Roads <= 0) return Result.Failure<Road>(DomainErrors.DomainError.MissingRoad);

        var result = Board.BuildRoad(playerId, edge);
        if (result.IsFailure) return Result.Failure<Road>(result.Error);

        playerBag.ApplyResourceAmounts(roadCost.Invert());
        playerBag.Roads -= 1;

        Bank.ApplyResourceAmounts(roadCost);

        AddDomainEvent(new RoadBuiltDomainEvent(Id, playerId, result.Value));

        return result;
    }

    public Result<PopulationCenter> BuildSettlement(PlayerId playerId, Vertex vertex)
    {
        if (CurrentPlayerId() != playerId) return Result.Failure<PopulationCenter>(DomainErrors.DomainError.WrongTurn);
        if (GamePhase != GamePhase.TradeBuild) return Result.Failure<PopulationCenter>(DomainErrors.DomainError.WrongGamePhase);

        var playerBag = Players.First(p => p.Id == playerId).ResourceBag;

        List<ResourceAmount> settlementPrice = PriceCalculator.SettlementPrice();
        if (!playerBag.HasAtLeast(settlementPrice)) return Result.Failure<PopulationCenter>(DomainErrors.DomainError.InsufficientResources);
        if (playerBag.Settlements <= 0) return Result.Failure<PopulationCenter>(DomainErrors.DomainError.MissingSettlement);

        var result = Board.PlaceSettlement(playerId, vertex);
        if (result.IsFailure) return Result.Failure<PopulationCenter>(result.Error);

        playerBag.ApplyResourceAmounts(settlementPrice.Invert());
        playerBag.Settlements -= 1;

        Bank.ApplyResourceAmounts(settlementPrice);

        AddDomainEvent(new SettlementBuiltDomainEvent(Id, playerId, result.Value));

        return result;
    }

    public Result<PopulationCenter> BuildCity(PlayerId playerId, Vertex vertex)
    {
        if (CurrentPlayerId() != playerId) return Result.Failure<PopulationCenter>(DomainErrors.DomainError.WrongTurn);
        if (GamePhase != GamePhase.TradeBuild) return Result.Failure<PopulationCenter>(DomainErrors.DomainError.WrongGamePhase);

        var playerBag = Players.First(p => p.Id == playerId).ResourceBag;

        List<ResourceAmount> cityPrice = PriceCalculator.CityPrice();
        if (!playerBag.HasAtLeast(cityPrice)) return Result.Failure<PopulationCenter>(DomainErrors.DomainError.InsufficientResources);
        if (playerBag.Cities <= 0) return Result.Failure<PopulationCenter>(DomainErrors.DomainError.MissingCity);

        var result = Board.UpgradeToCity(playerId, vertex);
        if (result.IsFailure) return Result.Failure<PopulationCenter>(result.Error);

        playerBag.ApplyResourceAmounts(cityPrice.Invert());
        playerBag.Cities -= 1;
        playerBag.Settlements += 1;

        Bank.ApplyResourceAmounts(cityPrice);

        AddDomainEvent(new SettlementUpgradedToCityDomainEvent(Id, playerId, result.Value));

        return result;
    }

    public Result<DevelopmentCardType> BuyDevelopmentCard(PlayerId playerId)
    {
        if (CurrentPlayerId() != playerId) return Result.Failure<DevelopmentCardType>(DomainErrors.DomainError.WrongTurn);
        if (GamePhase != GamePhase.TradeBuild) return Result.Failure<DevelopmentCardType>(DomainErrors.DomainError.WrongGamePhase);

        var playerBag = Players.First(p => p.Id == playerId).ResourceBag;

        List<ResourceAmount> devCardPrice = PriceCalculator.DevelopmentCardPrice();
        if (!playerBag.HasAtLeast(devCardPrice)) return Result.Failure<DevelopmentCardType>(DomainErrors.DomainError.InsufficientResources);
        if (playerBag.DevelopmentCards.Count > 3) return Result.Failure<DevelopmentCardType>(DomainErrors.DomainError.TooManyDevelopmentCards);
        if (Bank.DevelopmentCards.Count <= 0) return Result.Failure<DevelopmentCardType>(DomainErrors.DomainError.InsufficientBankDevelopmentCards);

        DevelopmentCardType developmentCardType = Bank.DrawRandomDevelopmentCard();
        playerBag.AddSingleDevelopmentCard(developmentCardType);

        playerBag.ApplyResourceAmounts(devCardPrice.Invert());
        Bank.ApplyResourceAmounts(devCardPrice);

        AddDomainEvent(new DevelopmentCardPurchasedDomainEvent(Id, playerId, developmentCardType));

        return Result.Success(developmentCardType);
    }
}
