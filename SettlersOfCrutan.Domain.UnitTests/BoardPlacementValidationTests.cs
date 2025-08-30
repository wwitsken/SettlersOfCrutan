using SettlersOfCrutan.Domain.Games;
using SettlersOfCrutan.Domain.Games.Coordinates;

namespace SettlersOfCrutan.Domain.UnitTests;
public class BoardPlacementValidationTests
{
    private static PlayerId NewPlayer() => new() { Value = 0 };

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
        var v1 = VertexFactory.GetAdjacentVertices(v0).First();

        // Determine the edge shared by v0 and v1: the two common hexes
        var s0 = new[] { v0.HexCoord1, v0.HexCoord2, v0.HexCoord3 };
        var s1 = new[] { v1.HexCoord1, v1.HexCoord2, v1.HexCoord3 };
        var shared = s0.Intersect(s1).ToList();
        Assert.Equal(2, shared.Count);
        var e0 = new Edge(shared[0], shared[1]);

        // Seed a connected road for the owner so BuildRoad connectivity passes
        HexCoord neighborOfShared0 = shared[0]
            .GetAdjacentHexCoords()
            .Values
            .First(v => !v.Equals(shared[1]));
        board.Roads.Add(new Road(new Edge(shared[0], neighborOfShared0)) { OwnerId = owner });

        // Build road for e0
        var road = board.BuildRoad(owner, e0);
        Assert.True(road.IsSuccess);

        // Attempt again using reversed order should fail
        var r2 = board.BuildRoad(owner, new Edge(shared[1], shared[0]));
        Assert.True(r2.IsFailure);
        Assert.Equal("RoadBuild", r2.Error.Code);
        Assert.Contains("already has a road", r2.Error.Message, StringComparison.OrdinalIgnoreCase);

        // Only one road added for that edge (count only normalized matches)
        var norm = e0.Normalize();
        Assert.Equal(1, board.Roads.Count(r => r.EdgeCoordinate.Normalize().Equals(norm)));
    }
}
