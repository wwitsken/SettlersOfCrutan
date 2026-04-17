using SettlersOfCrutan.Domain.Games;
using SettlersOfCrutan.Domain.Games.Boards;
using SettlersOfCrutan.Domain.Games.Boards.Coordinates;

namespace SettlersOfCrutan.Domain.UnitTests;

public class EdgeFactoryConnectivityTests
{
    [Fact]
    public void ConnectsToEdge_OppositeEdgesOnSameHex_ShareNoVertex()
    {
        var hex = new HexCoord(0, 0, 0);
        var north = EdgeFactory.FromHexEdge(hex, EdgeDirection.North).Normalize();
        var south = EdgeFactory.FromHexEdge(hex, EdgeDirection.South).Normalize();
        Assert.False(EdgeFactory.ConnectsToEdge(north, south));
    }

    [Fact]
    public void ConnectsToEdge_TwoDistinctRoadEdgesAtSameVertex_ShareAVertex()
    {
        var hex = new HexCoord(0, 0, 0);
        var v = VertexFactory.FromHexCorner(hex, HexCornerDirection.NW).Normalize();
        var inc = VertexFactory.IncidentRoadEdges(v).Take(2).ToArray();
        Assert.Equal(2, inc.Length);
        Assert.True(EdgeFactory.ConnectsToEdge(inc[0], inc[1]));
    }

    [Fact]
    public void ConnectsToEdge_AdjacentNW_and_SW_edges_on_same_hex_share_vertex()
    {
        var h = new HexCoord(-1, 0, 1);
        var road = new Edge(new HexCoord(-2, 0, 2), h).Normalize();
        var candidate = new Edge(h, new HexCoord(-2, 1, 1)).Normalize();
        Assert.True(EdgeFactory.ConnectsToEdge(road, candidate));
    }
}

public class BoardPlacementValidationTests
{
    private static PlayerId NewPlayer() => new() { Value = "123" };

    private static Edge FirstIncidentRoadEdge(Vertex v) =>
        VertexFactory.IncidentRoadEdges(v.Normalize()).First();

    [Fact]
    public void SettlementPlacement_RejectsDuplicateAcrossEquivalentVertexCoords()
    {
        var board = new Board();
        var owner = NewPlayer();

        var hex = new HexCoord(0, 0, 0);
        var v0 = VertexFactory.FromHexCorner(hex, HexCornerDirection.NE);
        var e0 = FirstIncidentRoadEdge(v0);

        // Place initial settlement
        var r1 = board.PlaceInitialSettlementAndRoad(owner, v0, e0);
        Assert.True(r1.IsSuccess);

        // Construct an equivalent vertex coordinate (same three hexes, different order)
        var vEq = new Vertex(v0.HexCoord2, v0.HexCoord1, v0.HexCoord3);
        var e1 = FirstIncidentRoadEdge(vEq);

        // Try to place another settlement at the same logical vertex
        var r2 = board.PlaceInitialSettlementAndRoad(owner, vEq, e1);
        Assert.True(r2.IsFailure);
        Assert.Equal("SettlementPlacement", r2.Error.Code);
        Assert.Contains("occupied", r2.Error.Message, StringComparison.OrdinalIgnoreCase);

        // Only one settlement overall
        Assert.Single(board.PopulationCenters);
    }

    [Fact]
    public void UpgradeToCityNoFail_ReplacesSettlement_KeepsSinglePopulationCenterAtVertex()
    {
        var board = new Board();
        var owner = NewPlayer();

        var hex = new HexCoord(0, 0, 0);
        var v0 = VertexFactory.FromHexCorner(hex, HexCornerDirection.NE);
        var e0 = FirstIncidentRoadEdge(v0);

        Assert.True(board.PlaceInitialSettlementAndRoad(owner, v0, e0).IsSuccess);
        Assert.Single(board.PopulationCenters);
        Assert.Equal(PopulationCenterLevel.Settlement, board.PopulationCenters[0].Level);

        var upgraded = board.UpgradeToCityNoFail(owner, v0.Normalize());

        Assert.Single(board.PopulationCenters);
        Assert.Same(upgraded, board.PopulationCenters[0]);
        Assert.Equal(PopulationCenterLevel.City, upgraded.Level);
        Assert.Equal(owner, upgraded.PlayerOwner);
        Assert.True(board.PopulationCenters[0].VertexCoordinate.Equals(v0.Normalize()));
    }

    [Fact]
    public void SettlementPlacement_DistanceRuleBlocksAdjacentRegardlessOfMaterialization()
    {
        var board = new Board();

        var owner = NewPlayer();

        var hex = new HexCoord(0, 0, 0);
        var v0 = VertexFactory.FromHexCorner(hex, HexCornerDirection.NE);
        var e0 = FirstIncidentRoadEdge(v0);

        // Place initial settlement
        var r1 = board.PlaceInitialSettlementAndRoad(owner, v0, e0);
        Assert.True(r1.IsSuccess);

        // Pick an adjacent vertex n such that the domain's adjacency function also sees v0 adjacent to n
        var candidateNeighbors = VertexFactory.GetAdjacentVertices(v0);
        var neighbor = candidateNeighbors.First(n => VertexFactory.GetAdjacentVertices(n).Contains(v0));

        var e1 = FirstIncidentRoadEdge(neighbor);
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
        var seedEdge = FirstIncidentRoadEdge(v0);
        var seedNorm = seedEdge.Normalize();
        var initial = board.PlaceInitialSettlementAndRoad(owner, v0, seedEdge);
        Assert.True(initial.IsSuccess);

        var v0Edges = VertexFactory.IncidentRoadEdges(v0).ToHashSet();

        // Pick an adjacent vertex whose shared edge with v0 is not the initial road (otherwise BuildRoad duplicates).
        Vertex v1 = default;
        Edge e0 = default;
        foreach (var cand in VertexFactory.GetAdjacentVertices(v0).OrderBy(v => v.ToString()))
        {
            foreach (var candEdge in VertexFactory.IncidentRoadEdges(cand))
            {
                if (candEdge.Equals(seedNorm)) continue;
                if (!v0Edges.Contains(candEdge)) continue;
                v1 = cand;
                e0 = new Edge(candEdge.HexCoord1, candEdge.HexCoord2);
                break;
            }

            if (v1 != default) break;
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

    [Fact]
    public void RoadPlacement_BlocksSecondRoadOnOppositeSideOfSameHexWithoutSharedVertex()
    {
        var board = new Board();
        var owner = NewPlayer();
        var hex = new HexCoord(0, 0, 0);
        var v0 = VertexFactory.FromHexCorner(hex, HexCornerDirection.NW).Normalize();
        var north = EdgeFactory.FromHexEdge(hex, EdgeDirection.North).Normalize();
        var seedEdge = VertexFactory.IncidentRoadEdges(v0).First(e => !e.Equals(north));
        Assert.True(board.PlaceInitialSettlementAndRoad(owner, v0, seedEdge).IsSuccess);

        var south = EdgeFactory.FromHexEdge(hex, EdgeDirection.South);

        Assert.True(board.BuildRoad(owner, north).IsSuccess);
        Assert.True(board.BuildRoad(owner, south).IsFailure);
    }

    [Fact]
    public void CanPlaceRoad_RoadBuildingSecondEdgeMayUseFirstEdgeBeforePlaced()
    {
        var board = new Board();
        var owner = NewPlayer();
        var hex = new HexCoord(0, 0, 0);
        var v0 = VertexFactory.FromHexCorner(hex, HexCornerDirection.NW).Normalize();
        var north = EdgeFactory.FromHexEdge(hex, EdgeDirection.North).Normalize();
        var seedEdge = VertexFactory.IncidentRoadEdges(v0).First(e => !e.Equals(north));
        Assert.True(board.PlaceInitialSettlementAndRoad(owner, v0, seedEdge).IsSuccess);

        Assert.True(board.CanPlaceRoad(owner, north).IsSuccess);

        var (epA, epB) = VertexFactory.EndpointsForEdge(north);
        var nv = v0.Normalize();
        var farEnd = epA.Equals(nv) ? epB : epA;
        var extend = VertexFactory.IncidentRoadEdges(farEnd).First(e => !e.Equals(north));

        Assert.False(board.CanPlaceRoad(owner, extend).IsSuccess);
        Assert.True(board.CanPlaceRoad(owner, extend, hypotheticalExtraOwnerRoads: [north]).IsSuccess);
    }
}
