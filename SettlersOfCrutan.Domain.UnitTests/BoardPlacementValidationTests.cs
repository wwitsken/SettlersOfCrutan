using SettlersOfCrutan.Domain.Games;
using SettlersOfCrutan.Domain.Games.Boards;
using SettlersOfCrutan.Domain.Games.Boards.Coordinates;

namespace SettlersOfCrutan.Domain.UnitTests;
public class BoardPlacementValidationTests
{
    private static PlayerId NewPlayer() => new() { Value = "123" };

    [Fact]
    public void SettlementPlacement_RejectsDuplicateAcrossEquivalentVertexCoords()
    {
        var board = new Board();
        var owner = NewPlayer();

        var hex = new HexCoord(0, 0, 0);
        var v0 = VertexFactory.FromHexCorner(hex, HexCornerDirection.NE);
        var e0 = new Edge() { HexCoord1 = v0.HexCoord2, HexCoord2 = v0.HexCoord1 };

        // Place initial settlement
        var r1 = board.PlaceInitialSettlementAndRoad(owner, v0, e0);
        Assert.True(r1.IsSuccess);

        // Construct an equivalent vertex coordinate (same three hexes, different order)
        var vEq = new Vertex(v0.HexCoord2, v0.HexCoord1, v0.HexCoord3);
        var e1 = new Edge() { HexCoord1 = v0.HexCoord1, HexCoord2 = v0.HexCoord3 };

        // Try to place another settlement at the same logical vertex
        var r2 = board.PlaceInitialSettlementAndRoad(owner, vEq, e1);
        Assert.True(r2.IsFailure);
        Assert.Equal("SettlementPlacement", r2.Error.Code);
        Assert.Contains("occupied", r2.Error.Message, StringComparison.OrdinalIgnoreCase);

        // Only one settlement overall
        Assert.Single(board.PopulationCenters);
    }

    [Fact]
    public void SettlementPlacement_DistanceRuleBlocksAdjacentRegardlessOfMaterialization()
    {
        var board = new Board();

        var owner = NewPlayer();

        var hex = new HexCoord(0, 0, 0);
        var v0 = VertexFactory.FromHexCorner(hex, HexCornerDirection.NE);
        var e0 = new Edge() { HexCoord1 = v0.HexCoord2, HexCoord2 = v0.HexCoord1 };

        // Place initial settlement
        var r1 = board.PlaceInitialSettlementAndRoad(owner, v0, e0);
        Assert.True(r1.IsSuccess);

        // Pick an adjacent vertex n such that the domain's adjacency function also sees v0 adjacent to n
        var candidateNeighbors = VertexFactory.GetAdjacentVertices(v0);
        var neighbor = candidateNeighbors.First(n => VertexFactory.GetAdjacentVertices(n).Contains(v0));

        var e1 = new Edge() { HexCoord1 = neighbor.HexCoord1, HexCoord2 = neighbor.HexCoord2 };
        var r2 = board.PlaceInitialSettlementAndRoad(owner, neighbor, e1);
        Assert.True(r2.IsFailure);
        Assert.Equal("SettlementPlacement", r2.Error.Code);
        Assert.Contains("Adjacent", r2.Error.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void RoadPlacement_CanonicalizesEdgeCoords()
    {
        var board = new Board();

        var owner = NewPlayer();

        var hex = new HexCoord(0, 0, 0);
        var v0 = VertexFactory.FromHexCorner(hex, HexCornerDirection.NE);
        var seedEdge = new Edge(v0.HexCoord1, v0.HexCoord2);
        var seedNorm = seedEdge.Normalize();
        var initial = board.PlaceInitialSettlementAndRoad(owner, v0, seedEdge);
        Assert.True(initial.IsSuccess);

        // Pick an adjacent vertex whose shared edge with v0 is not the initial road (otherwise BuildRoad duplicates).
        Vertex v1 = default;
        Edge e0 = default;
        foreach (var cand in VertexFactory.GetAdjacentVertices(v0).OrderBy(v => v.ToString()))
        {
            var s0 = new[] { v0.HexCoord1, v0.HexCoord2, v0.HexCoord3 };
            var s1 = new[] { cand.HexCoord1, cand.HexCoord2, cand.HexCoord3 };
            var shared = s0.Intersect(s1).ToList();
            Assert.Equal(2, shared.Count);
            var candEdge = new Edge(shared[0], shared[1]).Normalize();
            if (candEdge.Equals(seedNorm)) continue;
            v1 = cand;
            e0 = new Edge(shared[0], shared[1]);
            break;
        }

        Assert.NotEqual(default(Vertex), v1);

        // Build road for e0
        var road = board.BuildRoad(owner, e0);
        Assert.True(road.IsSuccess);

        // Attempt again using reversed order should fail
        var placedNorm = e0.Normalize();
        var r2 = board.BuildRoad(owner, new Edge(placedNorm.HexCoord2, placedNorm.HexCoord1));
        Assert.True(r2.IsFailure);
        Assert.Equal("RoadBuild", r2.Error.Code);
        Assert.Contains("already has a road", r2.Error.Message, StringComparison.OrdinalIgnoreCase);

        // Only one road added for that edge (count only normalized matches)
        Assert.Equal(1, board.Roads.Count(r => r.EdgeCoordinate.Normalize().Equals(placedNorm)));
    }
}
