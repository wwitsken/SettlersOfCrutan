using SettlersOfCrutan.Domain.Games;
using SettlersOfCrutan.Domain.Games.Boards;
using SettlersOfCrutan.Domain.Games.Boards.Coordinates;
using SettlersOfCrutan.Domain.Games.Resources;
using SettlersOfCrutan.Domain.Specifications.PlaceInitial;

namespace SettlersOfCrutan.Domain.UnitTests.Specifications;

public class PlaceInitialSpecTests
{
    private static readonly HexCoord Origin = new(0, 0, 0);
    private static readonly Vertex ValidVertex = VertexFactory.FromHexCorner(Origin, HexCornerDirection.NE);
    private static readonly Edge ValidEdge = VertexFactory.IncidentRoadEdges(ValidVertex).First();

    private static PlaceInitialContext MakeContext(
        GamePhase phase = GamePhase.Setup,
        PlayerId? currentPlayerId = null,
        PlayerId? actingPlayerId = null,
        Player? player = null,
        Board? board = null,
        Vertex? vertex = null,
        Edge? edge = null)
    {
        var pid = actingPlayerId ?? new PlayerId { Value = "p1" };
        return new PlaceInitialContext(
            phase,
            currentPlayerId ?? pid,
            pid,
            player ?? Player.Create("p1"),
            board ?? new Board(),
            vertex ?? ValidVertex,
            edge ?? ValidEdge);
    }

    [Fact]
    public void GameMustBeInSetupPhase_Setup_Succeeds()
    {
        var result = new GameMustBeInSetupPhase().IsSatisfiedBy(MakeContext(phase: GamePhase.Setup));
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void GameMustBeInSetupPhase_TradeBuild_Fails()
    {
        var result = new GameMustBeInSetupPhase().IsSatisfiedBy(MakeContext(phase: GamePhase.TradeBuild));
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
    public void PlayerMustHaveInitialPieces_FreshPlayer_Succeeds()
    {
        var result = new PlayerMustHaveInitialPieces().IsSatisfiedBy(MakeContext());
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void PlayerMustHaveInitialPieces_NoSettlements_Fails()
    {
        var player = Player.Create("p1");
        for (int i = 0; i < 5; i++) player.ConsumePiece(BuildableType.Settlement);

        var result = new PlayerMustHaveInitialPieces().IsSatisfiedBy(MakeContext(player: player));
        Assert.True(result.IsFailure);
        Assert.Equal("InitialPieces", result.Error.Code);
    }

    [Fact]
    public void BoardMustAllowInitialPlacement_ValidCoords_Succeeds()
    {
        var board = new Board();
        var result = new BoardMustAllowInitialPlacement().IsSatisfiedBy(
            MakeContext(board: board, vertex: ValidVertex, edge: ValidEdge));
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void BoardMustAllowInitialPlacement_OccupiedVertex_Fails()
    {
        var board = new Board();
        var owner = new PlayerId { Value = "other" };
        board.PlaceInitialSettlementAndRoad(owner, ValidVertex, ValidEdge);

        var result = new BoardMustAllowInitialPlacement().IsSatisfiedBy(
            MakeContext(board: board, vertex: ValidVertex, edge: ValidEdge));
        Assert.True(result.IsFailure);
    }
}
