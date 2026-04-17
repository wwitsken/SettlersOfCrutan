using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.Games.Boards;
using SettlersOfCrutan.Domain.Games.Boards.Coordinates;
using SettlersOfCrutan.Domain.Games.DomainEvents;
using SettlersOfCrutan.Domain.Games.Resources;
using SettlersOfCrutan.Domain.Specifications;
using RoadBuildingSpecs = SettlersOfCrutan.Domain.Specifications.PlayRoadBuilding;
using MonopolySpecs = SettlersOfCrutan.Domain.Specifications.PlayMonopoly;
using YearOfPlentySpecs = SettlersOfCrutan.Domain.Specifications.PlayYearOfPlenty;
using KnightSpecs = SettlersOfCrutan.Domain.Specifications.PlayKnight;

namespace SettlersOfCrutan.Domain.Games;
public partial class Game
{
    private static readonly ISpecification<RoadBuildingSpecs.PlayRoadBuildingContext>[] PlayRoadBuildingSpecifications =
    [
        new RoadBuildingSpecs.GameMustBeInTradeBuildPhase(),
        new RoadBuildingSpecs.MustBeCurrentPlayerTurn(),
        new RoadBuildingSpecs.PlayerMustHaveRoadBuildingCard(),
        new RoadBuildingSpecs.PlayerMustHaveTwoRoadPieces(),
        new RoadBuildingSpecs.FirstRoadMustBePlaceable(),
        new RoadBuildingSpecs.SecondRoadMustBePlaceable()
    ];

    public Result<(Road r1, Road r2)> PlayRoadBuilding(PlayerId playerId, Edge edge1, Edge edge2)
    {
        var player = Players.First(p => p.Id == playerId);
        var context = new RoadBuildingSpecs.PlayRoadBuildingContext(
            GamePhase, CurrentPlayerId(), playerId, player, Board, edge1, edge2);

        foreach (var spec in PlayRoadBuildingSpecifications)
        {
            var result = spec.IsSatisfiedBy(context);
            if (result.IsFailure)
                return Result.Failure<(Road, Road)>(result.Error);
        }

        if (edge1.HexCoords().Intersect(edge2.HexCoords()).Count() != 1)
        {
            // they must both connect to the existing network; already validated individually
        }

        var r1 = Board.PlaceRoadNoFail(playerId, edge1);
        var r2 = Board.PlaceRoadNoFail(playerId, edge2);

        player.UseDevCardToBank(BankDevCardHand, DevelopmentCardType.RoadBuilding);
        player.RecordNonKnightDevelopmentCardPlayed(DevelopmentCardType.RoadBuilding);
        player.ConsumePiece(BuildableType.Road, 2);

        AddDomainEvent(new RoadBuildingCardPlayedDomainEvent(Id, playerId, r1, r2));
        return Result.Success((r1, r2));
    }

    private static readonly ISpecification<MonopolySpecs.PlayMonopolyContext>[] PlayMonopolySpecifications =
    [
        new MonopolySpecs.GameMustBeInTradeBuildPhase(),
        new MonopolySpecs.MustBeCurrentPlayerTurn(),
        new MonopolySpecs.PlayerMustHaveMonopolyCard()
    ];

    public Result<int> PlayMonopoly(PlayerId playerId, ResourceCardType resourceType)
    {
        var player = Players.First(p => p.Id == playerId);
        var context = new MonopolySpecs.PlayMonopolyContext(GamePhase, CurrentPlayerId(), playerId, player);

        foreach (var spec in PlayMonopolySpecifications)
        {
            var result = spec.IsSatisfiedBy(context);
            if (result.IsFailure)
                return Result.Failure<int>(result.Error);
        }

        int totalStolen = 0;
        var stolenPerPlayer = new Dictionary<PlayerId, int>();
        foreach (var other in Players.Where(p => p.Id != playerId))
        {
            var moved = other.MoveAllOfResourceTo(BankResourceHand, resourceType);
            if (moved <= 0) continue;
            BankResourceHand.Transfer(player.ResourceHand, resourceType, moved);
            totalStolen += moved;
            stolenPerPlayer[other.Id] = moved;
        }

        player.UseDevCardToBank(BankDevCardHand, DevelopmentCardType.Monopoly);
        player.RecordNonKnightDevelopmentCardPlayed(DevelopmentCardType.Monopoly);
        AddDomainEvent(new MonopolyCardPlayedDomainEvent(Id, playerId, resourceType, stolenPerPlayer, totalStolen));
        return Result.Success(totalStolen);
    }

    private static readonly ISpecification<YearOfPlentySpecs.PlayYearOfPlentyContext>[] PlayYearOfPlentySpecifications =
    [
        new YearOfPlentySpecs.GameMustBeInTradeBuildPhase(),
        new YearOfPlentySpecs.MustBeCurrentPlayerTurn(),
        new YearOfPlentySpecs.PlayerMustHaveYearOfPlentyCard(),
        new YearOfPlentySpecs.BankMustHaveRequestedResources()
    ];

    public Result<(ResourceCardType t1, ResourceCardType t2)> PlayYearOfPlenty(PlayerId playerId, ResourceCardType t1, ResourceCardType t2)
    {
        var player = Players.First(p => p.Id == playerId);
        var context = new YearOfPlentySpecs.PlayYearOfPlentyContext(
            GamePhase, CurrentPlayerId(), playerId, player, t1, t2, BankResourceHand);

        foreach (var spec in PlayYearOfPlentySpecifications)
        {
            var result = spec.IsSatisfiedBy(context);
            if (result.IsFailure)
                return Result.Failure<(ResourceCardType, ResourceCardType)>(result.Error);
        }

        player.AddResource(t1); player.AddResource(t2);
        BankResourceHand.Subtract(t1); BankResourceHand.Subtract(t2);
        player.UseDevCardToBank(BankDevCardHand, DevelopmentCardType.YearOfPlenty);
        player.RecordNonKnightDevelopmentCardPlayed(DevelopmentCardType.YearOfPlenty);
        AddDomainEvent(new YearOfPlentyCardPlayedDomainEvent(Id, playerId, t1, t2));
        return Result.Success((t1, t2));
    }

    private static readonly ISpecification<KnightSpecs.PlayKnightContext>[] PlayKnightSpecifications =
    [
        new KnightSpecs.GameMustBeInTradeBuildPhase(),
        new KnightSpecs.MustBeCurrentPlayerTurn(),
        new KnightSpecs.PlayerMustHaveKnightCard()
    ];

    /// <summary>Consumes a knight dev card and enters robber resolution; call <see cref="ResolveRobber"/> next to move the robber and optionally steal.</summary>
    public Result<Nothing> PlayKnight(PlayerId playerId)
    {
        var player = Players.First(p => p.Id == playerId);
        var context = new KnightSpecs.PlayKnightContext(GamePhase, CurrentPlayerId(), playerId, player);

        foreach (var spec in PlayKnightSpecifications)
        {
            var result = spec.IsSatisfiedBy(context);
            if (result.IsFailure)
                return result;
        }

        player.UseDevCardToBank(BankDevCardHand, DevelopmentCardType.Knight);
        player.IncrementKnightsPlayed();
        GamePhase = GamePhase.ResolveRobber;
        AddDomainEvent(new KnightCardPlayedDomainEvent(Id, playerId));
        AddDomainEvent(new RobPlayerStartedDomainEvent(Id));
        return Result.Success();
    }
}
