using SettlersOfCrutan.Domain.Games;
using SettlersOfCrutan.Domain.Games.Boards;
using SettlersOfCrutan.Domain.Games.Boards.Coordinates;
using SettlersOfCrutan.Domain.Games.Resources;
using SettlersOfCrutan.Domain.Specifications.PlayRoadBuilding;

namespace SettlersOfCrutan.Domain.UnitTests.Specifications;

public class PlayRoadBuildingSpecTests
{
    private static readonly HexCoord Origin = new(0, 0, 0);
    private static readonly Vertex SeedVertex = VertexFactory.FromHexCorner(Origin, HexCornerDirection.NE);
    private static readonly Edge SeedEdge = VertexFactory.IncidentRoadEdges(SeedVertex).First();
    private static readonly Edge SecondEdgeFromVertex = VertexFactory.IncidentRoadEdges(SeedVertex).First(e => !e.Equals(SeedEdge));
    private static readonly Edge FarEdge = EdgeFactory.FromHexEdge(new HexCoord(3, -3, 0), EdgeDirection.North).Normalize();

    private static readonly PlayerId P1 = new() { Value = "p1" };

    private static PlayRoadBuildingContext MakeContext(
        GamePhase phase = GamePhase.TradeBuild,
        PlayerId? currentPlayerId = null,
        PlayerId? actingPlayerId = null,
        Player? actingPlayer = null,
        Board? board = null,
        Edge? edge1 = null,
        Edge? edge2 = null) =>
        new(
            phase,
            currentPlayerId ?? P1,
            actingPlayerId ?? P1,
            actingPlayer ?? Player.Create("p1"),
            board ?? new Board(),
            edge1 ?? SeedEdge,
            edge2 ?? SecondEdgeFromVertex);

    private static Board BoardWithSettlementForP1()
    {
        var board = new Board();
        board.PlaceSettlementNoFail(P1, SeedVertex);
        return board;
    }

    [Fact]
    public void GameMustBeInTradeBuildPhase_TradeBuild_Succeeds()
    {
        var result = new GameMustBeInTradeBuildPhase().IsSatisfiedBy(MakeContext(phase: GamePhase.TradeBuild));
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void GameMustBeInTradeBuildPhase_Setup_Fails()
    {
        var result = new GameMustBeInTradeBuildPhase().IsSatisfiedBy(MakeContext(phase: GamePhase.Setup));
        Assert.True(result.IsFailure);
        Assert.Equal("WrongGamePhase", result.Error.Code);
    }

    [Fact]
    public void MustBeCurrentPlayerTurn_SamePlayer_Succeeds()
    {
        var result = new MustBeCurrentPlayerTurn().IsSatisfiedBy(MakeContext(currentPlayerId: P1, actingPlayerId: P1));
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void MustBeCurrentPlayerTurn_DifferentPlayer_Fails()
    {
        var result = new MustBeCurrentPlayerTurn().IsSatisfiedBy(
            MakeContext(currentPlayerId: P1, actingPlayerId: new PlayerId { Value = "p2" }));
        Assert.True(result.IsFailure);
        Assert.Equal("WrongTurn", result.Error.Code);
    }

    [Fact]
    public void PlayerMustHaveRoadBuildingCard_HasCard_Succeeds()
    {
        var player = Player.Create("p1");
        player.AddDevCard(DevelopmentCardType.RoadBuilding);
        var result = new PlayerMustHaveRoadBuildingCard().IsSatisfiedBy(MakeContext(actingPlayer: player));
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void PlayerMustHaveRoadBuildingCard_NoCard_Fails()
    {
        var result = new PlayerMustHaveRoadBuildingCard().IsSatisfiedBy(MakeContext(actingPlayer: Player.Create("p1")));
        Assert.True(result.IsFailure);
        Assert.Equal("DevCard", result.Error.Code);
    }

    [Fact]
    public void PlayerMustHaveTwoRoadPieces_DefaultPlayer_Succeeds()
    {
        var result = new PlayerMustHaveTwoRoadPieces().IsSatisfiedBy(MakeContext(actingPlayer: Player.Create("p1")));
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void PlayerMustHaveTwoRoadPieces_OnlyOneRoadLeft_Fails()
    {
        var player = Player.Create("p1");
        for (var i = 0; i < 14; i++) player.ConsumePiece(BuildableType.Road);
        var result = new PlayerMustHaveTwoRoadPieces().IsSatisfiedBy(MakeContext(actingPlayer: player));
        Assert.True(result.IsFailure);
        Assert.Equal("DevCard", result.Error.Code);
    }

    [Fact]
    public void FirstRoadMustBePlaceable_ConnectedToSettlement_Succeeds()
    {
        var board = BoardWithSettlementForP1();
        var result = new FirstRoadMustBePlaceable().IsSatisfiedBy(
            MakeContext(board: board, actingPlayerId: P1, edge1: SeedEdge));
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void FirstRoadMustBePlaceable_Disconnected_Fails()
    {
        var board = BoardWithSettlementForP1();
        var result = new FirstRoadMustBePlaceable().IsSatisfiedBy(
            MakeContext(board: board, actingPlayerId: P1, edge1: FarEdge));
        Assert.True(result.IsFailure);
        Assert.Equal("RoadBuild", result.Error.Code);
    }

    [Fact]
    public void SecondRoadMustBePlaceable_BothConnected_Succeeds()
    {
        var board = BoardWithSettlementForP1();
        var result = new SecondRoadMustBePlaceable().IsSatisfiedBy(
            MakeContext(board: board, actingPlayerId: P1, edge1: SeedEdge, edge2: SecondEdgeFromVertex));
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void SecondRoadMustBePlaceable_SecondDisconnected_Fails()
    {
        var board = BoardWithSettlementForP1();
        var result = new SecondRoadMustBePlaceable().IsSatisfiedBy(
            MakeContext(board: board, actingPlayerId: P1, edge1: SeedEdge, edge2: FarEdge));
        Assert.True(result.IsFailure);
        Assert.Equal("RoadBuild", result.Error.Code);
    }
}
