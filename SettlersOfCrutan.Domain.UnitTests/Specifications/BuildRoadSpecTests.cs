using SettlersOfCrutan.Domain.Games;
using SettlersOfCrutan.Domain.Games.Boards;
using SettlersOfCrutan.Domain.Games.Boards.Coordinates;
using SettlersOfCrutan.Domain.Games.Resources;
using SettlersOfCrutan.Domain.Specifications.BuildRoad;

namespace SettlersOfCrutan.Domain.UnitTests.Specifications;

public class BuildRoadSpecTests
{
    private static readonly HexCoord Origin = new(0, 0, 0);
    private static readonly Vertex SeedVertex = VertexFactory.FromHexCorner(Origin, HexCornerDirection.NE);
    private static readonly Edge SeedEdge = VertexFactory.IncidentRoadEdges(SeedVertex).First();

    private static BuildRoadContext MakeContext(
        GamePhase phase = GamePhase.TradeBuild,
        PlayerId? currentPlayerId = null,
        PlayerId? actingPlayerId = null,
        Player? player = null,
        List<ResourceCardAmount>? cost = null,
        Board? board = null,
        Edge? edge = null)
    {
        var pid = actingPlayerId ?? new PlayerId { Value = "p1" };
        return new BuildRoadContext(
            phase,
            currentPlayerId ?? pid,
            pid,
            player ?? Player.Create(TestIds.User(1)),
            cost ?? [new ResourceCardAmount(ResourceCardType.Brick, 1), new ResourceCardAmount(ResourceCardType.Lumber, 1)],
            board ?? new Board(),
            edge ?? SeedEdge);
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
        Assert.Equal("WrongTurn", result.Error.Code);
    }

    [Fact]
    public void PlayerMustAffordRoad_HasResources_Succeeds()
    {
        var player = Player.Create(TestIds.User(1));
        player.AddResource(ResourceCardType.Brick, 1);
        player.AddResource(ResourceCardType.Lumber, 1);
        var cost = new List<ResourceCardAmount> { new(ResourceCardType.Brick, 1), new(ResourceCardType.Lumber, 1) };

        var result = new PlayerMustAffordRoad().IsSatisfiedBy(MakeContext(player: player, cost: cost));
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void PlayerMustAffordRoad_NoResources_Fails()
    {
        var player = Player.Create(TestIds.User(1));
        var cost = new List<ResourceCardAmount> { new(ResourceCardType.Brick, 1), new(ResourceCardType.Lumber, 1) };

        var result = new PlayerMustAffordRoad().IsSatisfiedBy(MakeContext(player: player, cost: cost));
        Assert.True(result.IsFailure);
        Assert.Equal("InsufficientResources", result.Error.Code);
    }

    [Fact]
    public void PlayerMustHaveRoadPiece_HasPieces_Succeeds()
    {
        var result = new PlayerMustHaveRoadPiece().IsSatisfiedBy(MakeContext());
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void PlayerMustHaveRoadPiece_NoPieces_Fails()
    {
        var player = Player.Create(TestIds.User(1));
        for (int i = 0; i < 15; i++) player.ConsumePiece(BuildableType.Road);

        var result = new PlayerMustHaveRoadPiece().IsSatisfiedBy(MakeContext(player: player));
        Assert.True(result.IsFailure);
        Assert.Equal("MissingRoad", result.Error.Code);
    }

    [Fact]
    public void BoardMustAllowRoadPlacement_ConnectedEdge_Succeeds()
    {
        var board = new Board();
        var pid = new PlayerId { Value = "p1" };
        board.PlaceInitialSettlementAndRoad(pid, SeedVertex, SeedEdge);
        var nextEdge = VertexFactory.IncidentRoadEdges(SeedVertex).First(e => !e.Equals(SeedEdge));

        var result = new BoardMustAllowRoadPlacement().IsSatisfiedBy(
            MakeContext(board: board, actingPlayerId: pid, edge: nextEdge));
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void BoardMustAllowRoadPlacement_DisconnectedEdge_Fails()
    {
        var board = new Board();
        var pid = new PlayerId { Value = "p1" };
        var farHex = new HexCoord(3, -3, 0);
        var farEdge = EdgeFactory.FromHexEdge(farHex, EdgeDirection.North).Normalize();

        var result = new BoardMustAllowRoadPlacement().IsSatisfiedBy(
            MakeContext(board: board, actingPlayerId: pid, edge: farEdge));
        Assert.True(result.IsFailure);
    }
}
