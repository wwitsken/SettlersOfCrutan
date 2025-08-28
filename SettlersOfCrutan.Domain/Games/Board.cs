using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.DomainErrors;
using SettlersOfCrutan.Domain.Games.Coordinates;

namespace SettlersOfCrutan.Domain.Games;
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

    public Result<PopulationCenter> PlaceInitialSettlement(PlayerId owner, Vertex coord)
    {
        if (PopulationCenters.Any(p => p.VertexCoordinate == coord))
            return Result<PopulationCenter>.Failure(new DomainError("SettlementPlacement", "Vertex already occupied"));

        // distance rule: no adjacent settlements (pure topology; independent of materialized edges/vertices)
        if (VertexFactory.GetAdjacentVertices(coord).Any(IsVertexOccupied))
            return Result<PopulationCenter>.Failure(new DomainError("SettlementPlacement", "Adjacent vertex occupied (distance rule)"));

        var entity = new PopulationCenter(coord) { PlayerOwner = owner };
        PopulationCenters.Add(entity);
        return Result<PopulationCenter>.Success(entity);
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

    public bool IsVertexOccupied(Vertex coord) =>
        PopulationCenters.Any(p => p.VertexCoordinate.Equals(coord));

    private bool HasAdjacentRoadOwnedBy(PlayerId owner, Vertex coord)
    {
        return Roads.Any(r => r.OwnerId.Equals(owner)
            && (r.EdgeCoordinate.HexCoord1.Equals(coord) || r.EdgeCoordinate.HexCoord2.Equals(coord)));
    }

    private bool IsEdgeBuildableBy(PlayerId owner, Edge edgeCoord)
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
}
