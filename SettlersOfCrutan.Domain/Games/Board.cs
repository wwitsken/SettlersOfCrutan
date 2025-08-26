using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.DomainErrors;

namespace SettlersOfCrutan.Domain.Games;
public record BoardId : BaseId;
public class Board : Entity<BoardId>
{
    public override BoardId Id { get; } = new();

    public Board()
    {
        Id.Value = Guid.NewGuid();
    }

    public List<Hex> Hexes { get; set; } = [];
    public List<Vertex> Vertices { get; set; } = [];
    public List<Edge> Edges { get; set; } = [];
    public List<Road> Roads { get; set; } = [];
    public List<Settlement> Settlements { get; set; } = [];
    public List<City> Cities { get; set; } = [];
    public List<Port> Ports { get; set; } = [];

    // Invariants helpers
    public Result<Settlement> PlaceSettlement(PlayerId owner, VertexCoord coord)
    {
        coord = HexTopology.Canonicalize(coord);
        var vertex = Vertices.FirstOrDefault(v => v.Coord.Equals(coord));
        if (vertex is null)
        {
            vertex = new Vertex { BoardId = Id, Coord = coord };
            Vertices.Add(vertex);
        }
        // must not be occupied
        if (vertex.SettlementId is not null || vertex.CityId is not null)
        {
            return Result<Settlement>.Failure(new DomainError("SettlementPlacement", "Vertex already occupied"));
        }
        // distance rule: no adjacent settlements (pure topology; independent of materialized edges/vertices)
        if (HexTopology.GetAdjacentVertices(coord).Any(vc => IsVertexOccupied(vc)))
        {
            return Result<Settlement>.Failure(new DomainError("SettlementPlacement", "Adjacent vertex occupied (distance rule)"));
        }
        // must connect to player's road unless during setup (caller enforces setup; here we enforce general rule via road adjacency)
        if (!HasAdjacentRoadOwnedBy(owner, coord))
        {
            return Result<Settlement>.Failure(new DomainError("SettlementPlacement", "No adjacent road for owner"));
        }
        var entity = new Settlement { BoardId = Id, OwnerId = owner, VertexId = vertex.Id };
        Settlements.Add(entity);
        vertex.SettlementId = entity.Id;
        return Result<Settlement>.Success(entity);
    }

    public Result<Settlement> PlaceInitialSettlement(PlayerId owner, VertexCoord coord)
    {
        coord = HexTopology.Canonicalize(coord);
        var vertex = Vertices.FirstOrDefault(v => v.Coord.Equals(coord));
        if (vertex is null)
        {
            vertex = new Vertex { BoardId = Id, Coord = coord };
            Vertices.Add(vertex);
        }
        // must not be occupied
        if (vertex.SettlementId is not null || vertex.CityId is not null)
        {
            return Result<Settlement>.Failure(new DomainError("SettlementPlacement", "Vertex already occupied"));
        }
        // distance rule: no adjacent settlements (pure topology; independent of materialized edges/vertices)
        if (HexTopology.GetAdjacentVertices(coord).Any(vc => IsVertexOccupied(vc)))
        {
            return Result<Settlement>.Failure(new DomainError("SettlementPlacement", "Adjacent vertex occupied (distance rule)"));
        }
        var entity = new Settlement { BoardId = Id, OwnerId = owner, VertexId = vertex.Id };
        Settlements.Add(entity);
        vertex.SettlementId = entity.Id;
        return Result<Settlement>.Success(entity);
    }

    public Result<City> UpgradeToCity(PlayerId owner, VertexCoord coord)
    {
        coord = HexTopology.Canonicalize(coord);
        var vertex = Vertices.FirstOrDefault(v => v.Coord.Equals(coord));
        if (vertex is null || vertex.SettlementId is null)
        {
            return Result<City>.Failure(new DomainError("CityUpgrade", "No settlement to upgrade"));
        }
        // must be owned by the same player
        var settlement = Settlements.FirstOrDefault(s => s.Id.Equals(vertex.SettlementId.Value));
        if (settlement is null || !settlement.OwnerId.Equals(owner))
        {
            return Result<City>.Failure(new DomainError("CityUpgrade", "Settlement not owned by player"));
        }
        // perform upgrade
        var city = new City { BoardId = Id, OwnerId = owner, VertexId = vertex.Id };
        Cities.Add(city);
        vertex.CityId = city.Id;
        return Result<City>.Success(city);
    }

    public Result<Road> BuildRoad(PlayerId owner, EdgeCoord coord)
    {
        coord = HexTopology.Canonicalize(coord);
        var edge = Edges.FirstOrDefault(e => e.Coord.Equals(coord));
        if (edge is null)
        {
            var (a, b) = GetVerticesForEdge(coord);
            edge = new Edge { BoardId = Id, Coord = coord, VertexAId = a.Id, VertexBId = b.Id };
            Edges.Add(edge);
        }
        if (edge.RoadId is not null)
        {
            return Result<Road>.Failure(new DomainError("RoadBuild", "Edge already has a road"));
        }
        // connectivity: adjacent to player's road or settlement/city at endpoints
        if (!IsEdgeBuildableBy(owner, edge))
        {
            return Result<Road>.Failure(new DomainError("RoadBuild", "Road not connected to player's network"));
        }
        var road = new Road { BoardId = Id, OwnerId = owner, EdgeId = edge.Id };
        Roads.Add(road);
        edge.RoadId = road.Id;
        return Result<Road>.Success(road);
    }

    private bool IsVertexOccupied(VertexCoord coord)
    {
        coord = HexTopology.Canonicalize(coord);
        var v = Vertices.FirstOrDefault(x => x.Coord.Equals(coord));
        if (v is null) return false;
        return v.SettlementId is not null || v.CityId is not null;
    }

    private bool HasAdjacentRoadOwnedBy(PlayerId owner, VertexCoord coord)
    {
        coord = HexTopology.Canonicalize(coord);
        foreach (var eCoord in GetEdgesForVertex(coord))
        {
            var e = Edges.FirstOrDefault(x => x.Coord.Equals(eCoord));
            if (e is not null && e.RoadId is not null)
            {
                var r = Roads.FirstOrDefault(rd => rd.Id.Equals(e.RoadId.Value));
                if (r is not null && r.OwnerId.Equals(owner)) return true;
            }
        }
        return false;
    }

    private bool IsEdgeBuildableBy(PlayerId owner, Edge edge)
    {
        // connected to player's existing road network
        bool connectedToRoad = Roads.Any(r => r.OwnerId.Equals(owner) && (r.EdgeId.Equals(edge.Id)));
        if (!connectedToRoad)
        {
            // if any adjacent edge has player's road, or endpoints have player's settlement/city
            var adjEdges = GetAdjacentEdges(edge.Coord);
            foreach (var eCoord in adjEdges)
            {
                var e = Edges.FirstOrDefault(x => x.Coord.Equals(eCoord));
                if (e is not null && e.RoadId is not null)
                {
                    var r = Roads.FirstOrDefault(rd => rd.Id.Equals(e.RoadId.Value));
                    if (r is not null && r.OwnerId.Equals(owner)) { connectedToRoad = true; break; }
                }
            }
            if (!connectedToRoad)
            {
                var a = Vertices.FirstOrDefault(v => v.Id.Equals(edge.VertexAId));
                var b = Vertices.FirstOrDefault(v => v.Id.Equals(edge.VertexBId));
                if (a is not null)
                {
                    var settlementOwned = a.SettlementId is not null && Settlements.Any(s => s.Id.Equals(a.SettlementId) && s.OwnerId.Equals(owner));
                    var cityOwned = a.CityId is not null && Cities.Any(c => c.Id.Equals(a.CityId) && c.OwnerId.Equals(owner));
                    if (settlementOwned || cityOwned) return true;
                }
                if (b is not null)
                {
                    var settlementOwned = b.SettlementId is not null && Settlements.Any(s => s.Id.Equals(b.SettlementId) && s.OwnerId.Equals(owner));
                    var cityOwned = b.CityId is not null && Cities.Any(c => c.Id.Equals(b.CityId) && c.OwnerId.Equals(owner));
                    if (settlementOwned || cityOwned) return true;
                }
            }
        }
        return connectedToRoad;
    }

    private IEnumerable<Vertex> GetAdjacentVertices(VertexCoord coord)
    {
        // Use topology to find neighbor coords and map to existing vertices if present
        foreach (var vCoord in HexTopology.GetAdjacentVertices(HexTopology.Canonicalize(coord)))
        {
            var existing = Vertices.FirstOrDefault(v => v.Coord.Equals(vCoord));
            if (existing is not null) yield return existing;
        }
    }

    private (Vertex a, Vertex b) GetVerticesForEdge(EdgeCoord coord)
    {
        coord = HexTopology.Canonicalize(coord);
        var (vaCoord, vbCoord) = HexTopology.GetEdgeVertices(coord);
        vaCoord = HexTopology.Canonicalize(vaCoord);
        vbCoord = HexTopology.Canonicalize(vbCoord);
        var va = Vertices.FirstOrDefault(v => v.Coord.Equals(vaCoord));
        if (va is null) { va = new Vertex { BoardId = Id, Coord = vaCoord }; Vertices.Add(va); }
        var vb = Vertices.FirstOrDefault(v => v.Coord.Equals(vbCoord));
        if (vb is null) { vb = new Vertex { BoardId = Id, Coord = vbCoord }; Vertices.Add(vb); }
        return (va, vb);
    }

    private IEnumerable<EdgeCoord> GetEdgesForVertex(VertexCoord coord)
        => HexTopology.GetVertexEdges(HexTopology.Canonicalize(coord));

    private IEnumerable<EdgeCoord> GetAdjacentEdges(EdgeCoord coord)
        => HexTopology.GetAdjacentEdges(HexTopology.Canonicalize(coord));
}
