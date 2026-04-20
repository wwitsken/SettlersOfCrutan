using SettlersOfCrutan.Domain.Games;
using SettlersOfCrutan.Domain.Games.Boards;
using SettlersOfCrutan.Domain.Games.Boards.Coordinates;
using SettlersOfCrutan.Domain.Specifications.EndTurn;

namespace SettlersOfCrutan.Domain.UnitTests.Specifications;

public class EndTurnSpecTests
{
    private static Vertex SampleVertex(int offset = 0)
    {
        var h0 = new HexCoord(offset, 0, -offset);
        var h1 = new HexCoord(offset + 1, 0, -offset - 1);
        var h2 = new HexCoord(offset, 1, -offset - 1);
        return new Vertex(h0, h1, h2).Normalize();
    }

    private static Road SampleRoad(PlayerId owner, int offset = 0)
    {
        var h0 = new HexCoord(offset, 0, -offset);
        var h1 = new HexCoord(offset + 1, 0, -offset - 1);
        var edge = new Edge(h0, h1).Normalize();
        var road = new Road(edge);
        road.OwnerId = owner;
        return road;
    }

    private static EndTurnContext MakeContext(
        GamePhase phase = GamePhase.TradeBuild,
        int round = 1,
        PlayerDirection direction = PlayerDirection.Clockwise,
        List<Road>? playerRoads = null,
        List<PopulationCenter>? playerPopulationCenters = null,
        PlayerId? currentPlayerId = null,
        PlayerId? actingPlayerId = null)
    {
        var pid = actingPlayerId ?? new PlayerId { Value = "p1" };
        return new EndTurnContext(
            phase,
            round,
            direction,
            playerRoads ?? [],
            playerPopulationCenters ?? [],
            currentPlayerId ?? pid,
            pid);
    }

    [Fact]
    public void GameMustBeInSetupOrTradeBuild_Setup_Succeeds()
    {
        var result = new GameMustBeInSetupOrTradeBuild().IsSatisfiedBy(MakeContext(phase: GamePhase.Setup));
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void GameMustBeInSetupOrTradeBuild_TradeBuild_Succeeds()
    {
        var result = new GameMustBeInSetupOrTradeBuild().IsSatisfiedBy(MakeContext(phase: GamePhase.TradeBuild));
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void GameMustBeInSetupOrTradeBuild_RollDice_Fails()
    {
        var result = new GameMustBeInSetupOrTradeBuild().IsSatisfiedBy(MakeContext(phase: GamePhase.RollDice));
        Assert.True(result.IsFailure);
        Assert.Equal("WrongGamePhase", result.Error.Code);
    }

    [Fact]
    public void MustBeCurrentPlayerTurn_SamePlayer_Succeeds()
    {
        var pid = new PlayerId { Value = "p1" };
        var result = new MustBeCurrentPlayerTurn().IsSatisfiedBy(MakeContext(currentPlayerId: pid, actingPlayerId: pid));
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void MustBeCurrentPlayerTurn_DifferentPlayer_Fails()
    {
        var result = new MustBeCurrentPlayerTurn().IsSatisfiedBy(
            MakeContext(currentPlayerId: new PlayerId { Value = "p1" }, actingPlayerId: new PlayerId { Value = "p2" }));
        Assert.True(result.IsFailure);
    }

    [Fact]
    public void DuringSetup_Clockwise_RequiresOneSettlementAndOneRoad()
    {
        var pid = new PlayerId { Value = "p1" };
        var road = SampleRoad(pid, 0);
        var settlement = PopulationCenter.CreateSettlement(SampleVertex(0), pid);
        var spec = new DuringSetupPhasePlayerMustHavePlacedInitialSettlementAndRoad();

        Assert.True(spec.IsSatisfiedBy(MakeContext(
            phase: GamePhase.Setup,
            direction: PlayerDirection.Clockwise,
            playerRoads: [road],
            playerPopulationCenters: [settlement],
            actingPlayerId: pid)).IsSuccess);

        Assert.True(spec.IsSatisfiedBy(MakeContext(
            phase: GamePhase.Setup,
            direction: PlayerDirection.Clockwise,
            playerRoads: [],
            playerPopulationCenters: [settlement],
            actingPlayerId: pid)).IsFailure);
    }

    [Fact]
    public void DuringSetup_CounterClockwise_RequiresTwoSettlementsAndTwoRoads()
    {
        var pid = new PlayerId { Value = "p1" };
        var roads = new[] { SampleRoad(pid, 0), SampleRoad(pid, 3) };
        var centers = new[]
        {
            PopulationCenter.CreateSettlement(SampleVertex(0), pid),
            PopulationCenter.CreateSettlement(SampleVertex(6), pid),
        };
        var spec = new DuringSetupPhasePlayerMustHavePlacedInitialSettlementAndRoad();

        Assert.True(spec.IsSatisfiedBy(MakeContext(
            phase: GamePhase.Setup,
            direction: PlayerDirection.CounterClockwise,
            playerRoads: [.. roads],
            playerPopulationCenters: [.. centers],
            actingPlayerId: pid)).IsSuccess);

        Assert.True(spec.IsSatisfiedBy(MakeContext(
            phase: GamePhase.Setup,
            direction: PlayerDirection.CounterClockwise,
            playerRoads: [roads[0]],
            playerPopulationCenters: [.. centers],
            actingPlayerId: pid)).IsFailure);
    }
}
