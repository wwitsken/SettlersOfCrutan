using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.Games;

namespace SettlersOfCrutan.Domain.UnitTests;
public class BoardPlacementValidationTests
{
    private static PlayerId NewPlayer() => new() { Value = Guid.NewGuid() };

    [Fact]
    public void SettlementPlacement_RejectsDuplicateAcrossEquivalentVertexCoords()
    {
        var board = new Board();
        var owner = NewPlayer();

        var hex = new AxialCoord(0, 0);
        VertexCoord v0 = new(hex, VertexCorner.TopRight);
        VertexCoord cv0 = HexTopology.Canonicalize(v0);

        // Place initial settlement
        var r1 = board.PlaceInitialSettlement(owner, v0);
        Assert.True(r1.IsSuccess);

        EdgeCoord roadCoord = new(hex, EdgeDirection.NE);
        Result<Road> road = board.BuildRoad(owner, roadCoord);
        Assert.True(road.IsSuccess);

        // Construct an equivalent vertex coordinate from a neighboring hex (same spot)
        int i = (int)VertexCorner.TopRight;
        var v1 = new VertexCoord(HexTopology.Neighbor(hex, (EdgeDirection)i), (VertexCorner)((i + 4) % 6));
        var r2 = board.PlaceSettlement(owner, v1);
        Assert.True(r2.IsFailure);
        Assert.Equal("SettlementPlacement", r2.Error.Code);
        Assert.Contains("occupied", r2.Error.Message, StringComparison.OrdinalIgnoreCase);

        // Only one vertex at that canonical coord and one settlement overall
        Assert.Equal(1, board.Vertices.Count(v => v.Coord.Equals(cv0)));
        Assert.Single(board.Settlements);
    }

    [Fact]
    public void SettlementPlacement_DistanceRuleBlocksAdjacentRegardlessOfMaterialization()
    {
        var board = new Board();
        var owner = NewPlayer();

        var hex = new AxialCoord(0, 0);
        var v0 = HexTopology.Canonicalize(new VertexCoord(hex, VertexCorner.TopRight));

        // Seed an adjacent road so first placement can succeed
        var seedEdge = HexTopology.GetVertexEdges(v0).First();
        var cSeedEdge = HexTopology.Canonicalize(seedEdge);
        var edgeEntity = new Edge { BoardId = board.Id, Coord = cSeedEdge };
        var roadEntity = new Road { BoardId = board.Id, OwnerId = owner, EdgeId = edgeEntity.Id };
        edgeEntity.RoadId = roadEntity.Id;
        board.Edges.Add(edgeEntity);
        board.Roads.Add(roadEntity);

        // Place initial settlement
        var r1 = board.PlaceInitialSettlement(owner, v0);
        Assert.True(r1.IsSuccess);

        // Try to place on an adjacent vertex (should fail due to distance rule) using canonical neighbor coord
        var neighbor = HexTopology.GetAdjacentVertices(v0).First();
        var r2 = board.PlaceSettlement(owner, neighbor);
        Assert.True(r2.IsFailure);
        Assert.Equal("SettlementPlacement", r2.Error.Code);
        Assert.Contains("Adjacent", r2.Error.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]

    public void RoadPlacement_CanonicalizesEdgeCoords()
    {
        var board = new Board();
        var owner = NewPlayer();

        var hex = new AxialCoord(0, 0);
        var e0 = new EdgeCoord(hex, EdgeDirection.NE);
        EdgeCoord ce0 = HexTopology.Canonicalize(e0);

        VertexCoord v0 = new(hex, VertexCorner.TopRight);
        VertexCoord cv0 = HexTopology.Canonicalize(v0);

        // Place initial settlement
        var initialSettlement = board.PlaceInitialSettlement(owner, v0);
        EdgeCoord roadCoord = new(hex, EdgeDirection.NE);

        Result<Road> road = board.BuildRoad(owner, roadCoord);
        Assert.True(road.IsSuccess);
        EdgeCoord roadCanonicalizedCoord = board.Edges.First(e => e.Id == road.Value.EdgeId).Coord;

        // Alternate representation of same edge from neighbor hex
        var eAlt = new EdgeCoord(
            HexTopology.Neighbor(roadCanonicalizedCoord.Hex, roadCanonicalizedCoord.Direction),
            HexTopology.Opposite(roadCanonicalizedCoord.Direction)
        );

        // Attempt again using the other representation should fail
        var r2 = board.BuildRoad(owner, e0);
        Assert.True(r2.IsFailure);
        Assert.Equal("RoadBuild", r2.Error.Code);
        Assert.Contains("already has a road", r2.Error.Message, StringComparison.OrdinalIgnoreCase);

        // Only one canonical edge exists for that spot and only one road added by build
        Assert.Equal(1, board.Edges.Count(e => e.Coord.Equals(ce0)));
        Assert.Single(board.Roads);
    }
}
