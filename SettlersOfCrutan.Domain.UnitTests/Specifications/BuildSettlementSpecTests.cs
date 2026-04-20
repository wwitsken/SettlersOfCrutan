using SettlersOfCrutan.Domain.Games;
using SettlersOfCrutan.Domain.Games.Boards;
using SettlersOfCrutan.Domain.Games.Boards.Coordinates;
using SettlersOfCrutan.Domain.Games.Resources;
using SettlersOfCrutan.Domain.Specifications.BuildSettlement;

namespace SettlersOfCrutan.Domain.UnitTests.Specifications;

public class BuildSettlementSpecTests
{
    private static readonly HexCoord Origin = new(0, 0, 0);
    private static readonly Vertex ValidVertex = VertexFactory.FromHexCorner(Origin, HexCornerDirection.NE);

    private static BuildSettlementContext MakeContext(
        GamePhase phase = GamePhase.TradeBuild,
        PlayerId? currentPlayerId = null,
        PlayerId? actingPlayerId = null,
        Player? player = null,
        List<ResourceCardAmount>? cost = null,
        Board? board = null,
        Vertex? vertex = null)
    {
        var pid = actingPlayerId ?? new PlayerId { Value = "p1" };
        return new BuildSettlementContext(
            phase,
            currentPlayerId ?? pid,
            pid,
            player ?? Player.Create(TestIds.User(1)),
            cost ?? [],
            board ?? new Board(),
            vertex ?? ValidVertex);
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
    public void PlayerMustAffordSettlement_HasResources_Succeeds()
    {
        var player = Player.Create(TestIds.User(1));
        player.AddResource(ResourceCardType.Brick, 1);
        player.AddResource(ResourceCardType.Lumber, 1);
        player.AddResource(ResourceCardType.Wool, 1);
        player.AddResource(ResourceCardType.Grain, 1);
        var cost = new List<ResourceCardAmount>
        {
            new(ResourceCardType.Brick, 1), new(ResourceCardType.Lumber, 1),
            new(ResourceCardType.Wool, 1), new(ResourceCardType.Grain, 1)
        };

        var result = new PlayerMustAffordSettlement().IsSatisfiedBy(MakeContext(player: player, cost: cost));
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void PlayerMustAffordSettlement_NoResources_Fails()
    {
        var player = Player.Create(TestIds.User(1));
        var cost = new List<ResourceCardAmount> { new(ResourceCardType.Brick, 1) };

        var result = new PlayerMustAffordSettlement().IsSatisfiedBy(MakeContext(player: player, cost: cost));
        Assert.True(result.IsFailure);
    }

    [Fact]
    public void PlayerMustHaveSettlementPiece_HasPieces_Succeeds()
    {
        var result = new PlayerMustHaveSettlementPiece().IsSatisfiedBy(MakeContext());
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void PlayerMustHaveSettlementPiece_NoPieces_Fails()
    {
        var player = Player.Create(TestIds.User(1));
        for (int i = 0; i < 5; i++) player.ConsumePiece(BuildableType.Settlement);

        var result = new PlayerMustHaveSettlementPiece().IsSatisfiedBy(MakeContext(player: player));
        Assert.True(result.IsFailure);
        Assert.Equal("MissingSettlement", result.Error.Code);
    }

    [Fact]
    public void BoardMustAllowSettlementPlacement_ValidVertex_Succeeds()
    {
        var board = new Board();
        var pid = new PlayerId { Value = "p1" };
        var v0 = ValidVertex.Normalize();
        var seedEdge = VertexFactory.IncidentRoadEdges(v0).First();
        Assert.True(board.PlaceInitialSettlementAndRoad(pid, v0, seedEdge).IsSuccess);

        var road1 = VertexFactory.IncidentRoadEdges(v0).First(e => !e.Normalize().Equals(seedEdge.Normalize()));
        Assert.True(board.BuildRoad(pid, road1).IsSuccess);

        var (a1, b1) = VertexFactory.EndpointsForEdge(road1);
        var vm = (a1.Normalize().Equals(v0) ? b1 : a1).Normalize();

        var v0Neighbors = VertexFactory.GetAdjacentVertices(v0).Select(n => n.Normalize()).ToHashSet();
        Vertex? target = null;
        Edge? road2 = null;
        foreach (var e in VertexFactory.IncidentRoadEdges(vm))
        {
            if (e.Normalize().Equals(road1.Normalize())) continue;
            var (a2, b2) = VertexFactory.EndpointsForEdge(e);
            var far = (a2.Normalize().Equals(vm) ? b2 : a2).Normalize();
            if (!v0Neighbors.Contains(far))
            {
                road2 = e;
                target = far;
                break;
            }
        }

        Assert.NotNull(road2);
        Assert.NotNull(target);
        Assert.True(board.BuildRoad(pid, road2.Value).IsSuccess);

        var result = new BoardMustAllowSettlementPlacement().IsSatisfiedBy(
            MakeContext(board: board, actingPlayerId: pid, vertex: target.Value));
        Assert.True(result.IsSuccess, result.IsFailure ? $"{result.Error.Code}: {result.Error.Message}" : "");
    }

    [Fact]
    public void BoardMustAllowSettlementPlacement_OccupiedVertex_Fails()
    {
        var board = new Board();
        var pid = new PlayerId { Value = "p1" };
        var seedEdge = VertexFactory.IncidentRoadEdges(ValidVertex).First();
        board.PlaceInitialSettlementAndRoad(pid, ValidVertex, seedEdge);

        var result = new BoardMustAllowSettlementPlacement().IsSatisfiedBy(
            MakeContext(board: board, actingPlayerId: pid, vertex: ValidVertex));
        Assert.True(result.IsFailure);
    }
}
