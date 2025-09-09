using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.DomainErrors;
using SettlersOfCrutan.Domain.Games.Boards.Coordinates;

namespace SettlersOfCrutan.Domain.Games.Boards;
public record BoardId : BaseId<Guid>;
public class Board : Entity<BoardId>
{
    public override BoardId Id { get; init; } = new() { Value = Guid.NewGuid() };

    public List<Hex> Hexes { get; set; } = [];
    public List<PopulationCenter> PopulationCenters { get; set; } = [];
    public List<Road> Roads { get; set; } = [];
    public List<Port> Ports { get; set; } = [];


    // Invariants helpers
    public Result<PopulationCenter> PlaceSettlement(PlayerId owner, Vertex coord)
    {
        if (PopulationCenters.Any(p => p.VertexCoordinate == coord))
            return Result<PopulationCenter>.Failure(new DomainError("SettlementPlacement", "Vertex already occupied"));

        // distance rule: no adjacent settlements (pure topology; independent of materialized edges/vertices)
        if (VertexFactory.GetAdjacentVertices(coord).Any(IsVertexOccupied))
            return Result<PopulationCenter>.Failure(new DomainError("SettlementPlacement", "Adjacent vertex occupied (distance rule)"));

        // must connect to player's road unless during setup (caller enforces setup; here we enforce general rule via road adjacency)
        if (!HasAdjacentRoadOwnedBy(owner, coord))
            return Result<PopulationCenter>.Failure(new DomainError("SettlementPlacement", "No adjacent road for owner"));

        var entity = new PopulationCenter(coord) { PlayerOwner = owner };
        PopulationCenters.Add(entity);
        return Result<PopulationCenter>.Success(entity);
    }

    public Result<(PopulationCenter, Road)> PlaceInitialSettlementAndRoad(PlayerId owner, Vertex vertex, Edge edge)
    {
        if (PopulationCenters.Any(p => p.VertexCoordinate == vertex))
            return Result<(PopulationCenter, Road)>.Failure(new DomainError("SettlementPlacement", "Vertex already occupied"));

        // distance rule: no adjacent settlements (pure topology; independent of materialized edges/vertices)
        if (VertexFactory.GetAdjacentVertices(vertex).Any(IsVertexOccupied))
            return Result<(PopulationCenter, Road)>.Failure(new DomainError("SettlementPlacement", "Adjacent vertex occupied (distance rule)"));

        var normEdge = edge.Normalize();
        if (Roads.Any(r => r.EdgeCoordinate.Normalize().Equals(normEdge)))
            return Result<(PopulationCenter, Road)>.Failure(new DomainError("RoadBuild", "Edge already has a road"));

        // connectivity: initial road must connect to initial settlement
        var edgeAndVertexConnected = vertex.HexCoords().Intersect(edge.HexCoords()).Count() == 2;
        if (!edgeAndVertexConnected)
            return Result<(PopulationCenter, Road)>.Failure(new DomainError("RoadBuildAndSettlement", "New road and settlement are not connected"));

        var road = new Road(normEdge) { OwnerId = owner };
        Roads.Add(road);

        var populationCenter = new PopulationCenter(vertex) { PlayerOwner = owner };
        PopulationCenters.Add(populationCenter);

        return Result<(PopulationCenter, Road)>.Success((populationCenter, road));
    }

    public Result<PopulationCenter> UpgradeToCity(PlayerId owner, Vertex coord)
    {
        if (!PopulationCenters.Any(p => p.VertexCoordinate == coord))
            return Result<PopulationCenter>.Failure(new DomainError("CityUpgrade", "No settlement to upgrade"));

        // must be owned by the same player
        var settlement = PopulationCenters.FirstOrDefault(s => s.PlayerOwner == owner && s.VertexCoordinate == coord);
        if (settlement is null)
        {
            return Result<PopulationCenter>.Failure(new DomainError("CityUpgrade", "Settlement not owned by player"));
        }
        // perform upgrade
        var city = new PopulationCenter(settlement.VertexCoordinate) { Level = PopulationCenterLevel.City, PlayerOwner = owner };
        PopulationCenters.Add(city);
        return Result<PopulationCenter>.Success(city);
    }

    public Result<Road> BuildRoad(PlayerId owner, Edge coord)
    {
        var norm = coord.Normalize();
        if (Roads.Any(r => r.EdgeCoordinate.Normalize().Equals(norm)))
            return Result<Road>.Failure(new DomainError("RoadBuild", "Edge already has a road"));

        // connectivity: adjacent to player's road or settlement/city at endpoints
        if (!IsEdgeBuildableBy(owner, coord))
            return Result<Road>.Failure(new DomainError("RoadBuild", "Road not connected to player's network"));

        var road = new Road(norm) { OwnerId = owner };
        Roads.Add(road);
        return Result<Road>.Success(road);
    }

    public Result<HexCoord> MoveRobber(HexCoord newCoord)
    {
        var hex = Hexes.FirstOrDefault(h => h.Coordinate.Equals(newCoord));
        if (hex is null)
            return Result<HexCoord>.Failure(new DomainError("RobberMove", "Hex does not exist"));
        if (hex.Resource == ResourceType.Desert)
            return Result<HexCoord>.Failure(new DomainError("RobberMove", "Cannot place robber on desert"));
        if (Hexes.Any(h => h.HasRobber && h.Coordinate.Equals(newCoord)))
            return Result<HexCoord>.Failure(new DomainError("RobberMove", "Robber already on this hex"));
        // Remove robber from current hex
        var currentHex = Hexes.FirstOrDefault(h => h.HasRobber);
        if (currentHex is not null) currentHex.HasRobber = false;
        // Place robber on new hex
        hex.HasRobber = true;
        return Result<HexCoord>.Success(newCoord);
    }

    public bool IsPlayerExposedToHex(HexCoord hexCoord, PlayerId playerId) =>
        PopulationCenters.Any(pc => pc.VertexCoordinate.HexCoords().Contains(hexCoord) && pc.PlayerOwner == playerId);

    public bool IsVertexOccupied(Vertex coord) =>
        PopulationCenters.Any(p => p.VertexCoordinate.Equals(coord));

    private bool HasAdjacentRoadOwnedBy(PlayerId owner, Vertex coord)
    {
        return Roads.Any(r => r.OwnerId.Equals(owner)
            && (r.EdgeCoordinate.HexCoord1.Equals(coord) || r.EdgeCoordinate.HexCoord2.Equals(coord)));
    }

    public bool IsEdgeBuildableBy(PlayerId owner, Edge edgeCoord)
    {
        // connected to player's existing road network
        bool connectedToRoad = Roads.Any(r => r.OwnerId.Equals(owner)
            && EdgeFactory.ConnectsToEdge(r.EdgeCoordinate, edgeCoord));

        bool connectedToPopulationCenter = PopulationCenters.Any(pc =>
        {
            var va = edgeCoord.HexCoord1;
            var vb = edgeCoord.HexCoord2;

            return (pc.VertexCoordinate.Equals(va) || pc.VertexCoordinate.Equals(vb))
                && pc.PlayerOwner.Equals(owner);
        });

        return connectedToRoad || connectedToPopulationCenter;
    }

    public bool CanMoveRobberTo(HexCoord newRobberHexCoord)
    {
        var hex = Hexes.FirstOrDefault(h => h.Coordinate.Equals(newRobberHexCoord));
        if (hex is null) return false;
        if (hex.Resource == ResourceType.Desert) return false;
        if (hex.HasRobber) return false;
        return true;
    }

    public static Board Create(List<Hex> hexes, List<Port> ports) => new()
    {
        Hexes = hexes,
        Ports = ports,
        PopulationCenters = [],
        Roads = [],
        Id = new() { Value = Guid.NewGuid() }
    };

}
