using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.Games.Boards;
using SettlersOfCrutan.Domain.Games.Boards.Coordinates;
using SettlersOfCrutan.Domain.Games.DomainEvents;
using SettlersOfCrutan.Domain.Games.Resources;
using SettlersOfCrutan.Domain.Specifications;
using PlaceInitialSpecs = SettlersOfCrutan.Domain.Specifications.PlaceInitial;
using BuildRoadSpecs = SettlersOfCrutan.Domain.Specifications.BuildRoad;
using BuildSettlementSpecs = SettlersOfCrutan.Domain.Specifications.BuildSettlement;
using BuildCitySpecs = SettlersOfCrutan.Domain.Specifications.BuildCity;
using BuyDevCardSpecs = SettlersOfCrutan.Domain.Specifications.BuyDevelopmentCard;

namespace SettlersOfCrutan.Domain.Games;
public partial class Game
{
    private static readonly ISpecification<PlaceInitialSpecs.PlaceInitialContext>[] PlaceInitialSpecifications =
    [
        new PlaceInitialSpecs.GameMustBeInSetupPhase(),
        new PlaceInitialSpecs.MustBeCurrentPlayerTurn(),
        new PlaceInitialSpecs.PlayerMustHaveInitialPieces(),
        new PlaceInitialSpecs.BoardMustAllowInitialPlacement()
    ];

    public Result<(PopulationCenter, Road)> PlaceInitial(PlayerId playerId, Vertex settlementVertex, Edge roadEdge, IDateTimeProvider clock)
    {
        var player = Players.First(p => p.Id == playerId);
        var context = new PlaceInitialSpecs.PlaceInitialContext(GamePhase, CurrentPlayerId(), playerId, player, Board, settlementVertex, roadEdge);

        foreach (var spec in PlaceInitialSpecifications)
        {
            var result = spec.IsSatisfiedBy(context);
            if (result.IsFailure)
                return Result.Failure<(PopulationCenter, Road)>(result.Error);
        }

        var (settlement, road) = Board.PlaceInitialSettleAndRoadNoFail(playerId, settlementVertex, roadEdge);
        player.ConsumePiece(BuildableType.Settlement);
        player.ConsumePiece(BuildableType.Road);

        EndTurn(playerId, clock);
        AddDomainEvent(new InitialSettlementAndRoadPlacedDomainEvent(Id, playerId, settlement, road));
        return Result.Success((settlement, road));
    }

    private static readonly ISpecification<BuildRoadSpecs.BuildRoadContext>[] BuildRoadSpecifications =
    [
        new BuildRoadSpecs.GameMustBeInTradeBuildPhase(),
        new BuildRoadSpecs.MustBeCurrentPlayerTurn(),
        new BuildRoadSpecs.PlayerMustAffordRoad(),
        new BuildRoadSpecs.PlayerMustHaveRoadPiece(),
        new BuildRoadSpecs.BoardMustAllowRoadPlacement()
    ];

    public Result<Road> BuildRoad(IPriceCalculator priceCalculator, PlayerId playerId, Edge edge)
    {
        var player = Players.First(p => p.Id == playerId);
        var cost = priceCalculator.RoadPrice();
        var context = new BuildRoadSpecs.BuildRoadContext(GamePhase, CurrentPlayerId(), playerId, player, cost, Board, edge);

        foreach (var spec in BuildRoadSpecifications)
        {
            var result = spec.IsSatisfiedBy(context);
            if (result.IsFailure)
                return Result.Failure<Road>(result.Error);
        }

        var road = Board.PlaceRoadNoFail(playerId, edge);
        player.PayResourcesTo(BankResourceHand, cost);
        player.ConsumePiece(BuildableType.Road);

        AddDomainEvent(new RoadBuiltDomainEvent(Id, playerId, road));
        return Result.Success(road);
    }

    private static readonly ISpecification<BuildSettlementSpecs.BuildSettlementContext>[] BuildSettlementSpecifications =
    [
        new BuildSettlementSpecs.GameMustBeInTradeBuildPhase(),
        new BuildSettlementSpecs.MustBeCurrentPlayerTurn(),
        new BuildSettlementSpecs.PlayerMustAffordSettlement(),
        new BuildSettlementSpecs.PlayerMustHaveSettlementPiece(),
        new BuildSettlementSpecs.BoardMustAllowSettlementPlacement()
    ];

    public Result<PopulationCenter> BuildSettlement(IPriceCalculator priceCalculator, PlayerId playerId, Vertex vertex)
    {
        var player = Players.First(p => p.Id == playerId);
        var cost = priceCalculator.SettlementPrice();
        var context = new BuildSettlementSpecs.BuildSettlementContext(GamePhase, CurrentPlayerId(), playerId, player, cost, Board, vertex);

        foreach (var spec in BuildSettlementSpecifications)
        {
            var result = spec.IsSatisfiedBy(context);
            if (result.IsFailure)
                return Result.Failure<PopulationCenter>(result.Error);
        }

        var settlement = Board.PlaceSettlementNoFail(playerId, vertex);
        player.PayResourcesTo(BankResourceHand, cost);
        player.ConsumePiece(BuildableType.Settlement);

        AddDomainEvent(new SettlementBuiltDomainEvent(Id, playerId, settlement));
        return Result.Success(settlement);
    }

    private static readonly ISpecification<BuildCitySpecs.BuildCityContext>[] BuildCitySpecifications =
    [
        new BuildCitySpecs.GameMustBeInTradeBuildPhase(),
        new BuildCitySpecs.MustBeCurrentPlayerTurn(),
        new BuildCitySpecs.PlayerMustAffordCity(),
        new BuildCitySpecs.PlayerMustHaveCityPiece(),
        new BuildCitySpecs.BoardMustAllowCityUpgrade()
    ];

    public Result<PopulationCenter> BuildCity(IPriceCalculator priceCalculator, PlayerId playerId, Vertex vertex)
    {
        var player = Players.First(p => p.Id == playerId);
        var cost = priceCalculator.CityPrice();
        var context = new BuildCitySpecs.BuildCityContext(GamePhase, CurrentPlayerId(), playerId, player, cost, Board, vertex);

        foreach (var spec in BuildCitySpecifications)
        {
            var result = spec.IsSatisfiedBy(context);
            if (result.IsFailure)
                return Result.Failure<PopulationCenter>(result.Error);
        }

        var city = Board.UpgradeToCityNoFail(playerId, vertex);
        player.PayResourcesTo(BankResourceHand, cost);
        player.ConsumePiece(BuildableType.City);
        player.ReturnPiece(BuildableType.Settlement);

        AddDomainEvent(new SettlementUpgradedToCityDomainEvent(Id, playerId, city));
        return Result.Success(city);
    }

    private static readonly ISpecification<BuyDevCardSpecs.BuyDevelopmentCardContext>[] BuyDevelopmentCardSpecifications =
    [
        new BuyDevCardSpecs.GameMustBeInTradeBuildPhase(),
        new BuyDevCardSpecs.MustBeCurrentPlayerTurn(),
        new BuyDevCardSpecs.PlayerMustAffordDevelopmentCard(),
        new BuyDevCardSpecs.PlayerMustNotExceedDevCardLimit(),
        new BuyDevCardSpecs.BankMustHaveDevelopmentCards()
    ];

    public Result<DevelopmentCardType> BuyDevelopmentCard(IPriceCalculator priceCalculator, PlayerId playerId)
    {
        var player = Players.First(p => p.Id == playerId);
        var cost = priceCalculator.DevelopmentCardPrice();
        var context = new BuyDevCardSpecs.BuyDevelopmentCardContext(
            GamePhase, CurrentPlayerId(), playerId, player, cost,
            player.DevCardHand.Cards.Count, BankDevCardHand.Cards.Count);

        foreach (var spec in BuyDevelopmentCardSpecifications)
        {
            var result = spec.IsSatisfiedBy(context);
            if (result.IsFailure)
                return Result.Failure<DevelopmentCardType>(result.Error);
        }

        var drawn = BankDevCardHand.DrawRandom();
        player.AddDevCard(drawn);
        player.PayResourcesTo(BankResourceHand, cost);

        AddDomainEvent(new DevelopmentCardPurchasedDomainEvent(Id, playerId, drawn));
        return Result.Success(drawn);
    }
}
