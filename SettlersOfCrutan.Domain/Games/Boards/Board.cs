using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.DomainErrors;
using SettlersOfCrutan.Domain.Games.Boards.Coordinates;
using SettlersOfCrutan.Domain.Games.Resources;
using System.Text.Json.Serialization;

namespace SettlersOfCrutan.Domain.Games.Boards;
public record BoardId : BaseId<Guid>;
public class Board : Entity<BoardId>
{
    public override BoardId Id { get; init; } = new() { Value = Guid.NewGuid() };

    private readonly List<Hex> _hexes = [];
    private readonly List<PopulationCenter> _populationCenters = [];
    private readonly List<Road> _roads = [];
    private readonly List<Port> _ports = [];
    public IReadOnlyList<Hex> Hexes => [.. _hexes];
    public IReadOnlyList<PopulationCenter> PopulationCenters => [.. _populationCenters];
    public IReadOnlyList<Road> Roads => [.. _roads];
    public IReadOnlyList<Port> Ports => [.. _ports];

    // Public parameterless constructor for tests / tooling
    public Board() { }

    [JsonConstructor]
    private Board(BoardId id,
                  IReadOnlyList<Hex> hexes,
                  IReadOnlyList<PopulationCenter> populationCenters,
                  IReadOnlyList<Road> roads,
                  IReadOnlyList<Port> ports)
    {
        Id = id;
        _hexes = [.. hexes];
        _populationCenters = [.. populationCenters];
        _roads = [.. roads];
        _ports = [.. ports];
    }
    public static Board Create(List<Hex> hexes, List<Port> ports) => new(new() { Value = Guid.NewGuid() },
                                                                         hexes,
                                                                         [],
                                                                         [],
                                                                         ports);

    // --- Pure validation methods (Result = precondition outcome) ---
    public Result<Nothing> CanPlaceRoad(PlayerId owner, Edge coord, bool isInitialPlacement = false)
    {
        var norm = coord.Normalize();
        if (Roads.Any(r => r.EdgeCoordinate.Normalize().Equals(norm)))
            return Result.Failure<Nothing>(new DomainError("RoadBuild", "Edge already has a road"));
        if (!IsEdgeBuildableBy(owner, coord) && !isInitialPlacement)
            return Result.Failure<Nothing>(new DomainError("RoadBuild", "Road not connected to player's network"));
        return Result.Success();
    }

    public Result<Nothing> CanPlaceSettlement(PlayerId owner, Vertex coord, bool isInitialPlacement = false)
    {
        if (PopulationCenters.Any(p => p.VertexCoordinate == coord))
            return Result.Failure<Nothing>(new DomainError("SettlementPlacement", "Vertex already occupied"));
        if (VertexFactory.GetAdjacentVertices(coord).Any(IsVertexOccupied))
            return Result.Failure<Nothing>(new DomainError("SettlementPlacement", "Adjacent vertex occupied (distance rule)"));
        if (!HasAdjacentRoadOwnedBy(owner, coord) && !isInitialPlacement)
            return Result.Failure<Nothing>(new DomainError("SettlementPlacement", "No adjacent road for owner"));
        return Result.Success();
    }

    public Result<Nothing> CanUpgradeToCity(PlayerId owner, Vertex coord)
    {
        if (!PopulationCenters.Any(p => p.VertexCoordinate == coord))
            return Result.Failure<Nothing>(new DomainError("CityUpgrade", "No settlement to upgrade"));
        var settlement = PopulationCenters.FirstOrDefault(s => s.PlayerOwner == owner && s.VertexCoordinate == coord && s.Level == PopulationCenterLevel.Settlement);
        if (settlement is null) return Result.Failure<Nothing>(new DomainError("CityUpgrade", "Settlement not owned by player"));
        return Result.Success();
    }

    public Result<Nothing> CanPlaceInitialSettleAndRoad(Vertex vertex, Edge edge)
    {
        if (PopulationCenters.Any(p => p.VertexCoordinate == vertex))
            return Result.Failure<Nothing>(new DomainError("SettlementPlacement", "Vertex already occupied"));
        if (VertexFactory.GetAdjacentVertices(vertex).Any(IsVertexOccupied))
            return Result.Failure<Nothing>(new DomainError("SettlementPlacement", "Adjacent vertex occupied (distance rule)"));
        var normEdge = edge.Normalize();
        if (Roads.Any(r => r.EdgeCoordinate.Normalize().Equals(normEdge)))
            return Result.Failure<Nothing>(new DomainError("RoadBuild", "Edge already has a road"));
        var edgeAndVertexConnected = vertex.HexCoords().Intersect(edge.HexCoords()).Count() == 2;
        if (!edgeAndVertexConnected)
            return Result.Failure<Nothing>(new DomainError("RoadBuildAndSettlement", "New road and settlement are not connected"));
        return Result.Success();
    }

    // --- No-fail mutators (assume Can* already called) ---
    public Road PlaceRoadNoFail(PlayerId owner, Edge coord)
    {
        var norm = coord.Normalize();
        var road = new Road(norm) { OwnerId = owner };
        _roads.Add(road);
        return road;
    }

    public PopulationCenter PlaceSettlementNoFail(PlayerId owner, Vertex coord)
    {
        var entity = new PopulationCenter(coord) { PlayerOwner = owner };
        _populationCenters.Add(entity);
        return entity;
    }

    public PopulationCenter UpgradeToCityNoFail(PlayerId owner, Vertex coord)
    {
        var city = new PopulationCenter(coord) { Level = PopulationCenterLevel.City, PlayerOwner = owner };
        _populationCenters.Add(city);
        return city;
    }

    public (PopulationCenter settlement, Road road) PlaceInitialSettleAndRoadNoFail(PlayerId owner, Vertex vertex, Edge edge)
    {
        var road = new Road(edge.Normalize()) { OwnerId = owner };
        _roads.Add(road);
        var settlement = new PopulationCenter(vertex) { PlayerOwner = owner };
        _populationCenters.Add(settlement);
        return (settlement, road);
    }

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
        _populationCenters.Add(entity);
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
        _roads.Add(road);

        var populationCenter = new PopulationCenter(vertex) { PlayerOwner = owner };
        _populationCenters.Add(populationCenter);

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
        _populationCenters.Add(city);
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
        _roads.Add(road);
        return Result<Road>.Success(road);
    }

    public Result<HexCoord> MoveRobber(HexCoord newCoord)
    {
        var hex = Hexes.FirstOrDefault(h => h.Coordinate.Equals(newCoord));
        if (hex is null)
            return Result<HexCoord>.Failure(new DomainError("RobberMove", "Hex does not exist"));
        if (hex.Resource == ResourceCardType.Desert)
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
        var n = coord.Normalize();
        return Roads.Any(r =>
            r.OwnerId.Equals(owner) && SettlementTouchesEdge(n, r.EdgeCoordinate));
    }

    public bool IsEdgeBuildableBy(PlayerId owner, Edge edgeCoord)
    {
        bool connectedToRoad = Roads.Any(r => r.OwnerId.Equals(owner)
            && EdgeFactory.ConnectsToEdge(r.EdgeCoordinate, edgeCoord));

        bool connectedToPopulationCenter = PopulationCenters.Any(pc =>
            pc.PlayerOwner.Equals(owner)
            && SettlementTouchesEdge(pc.VertexCoordinate, edgeCoord));

        return connectedToRoad || connectedToPopulationCenter;
    }

    /// <summary>
    /// True when the settlement sits on an endpoint of the edge (both edge hexes are in the vertex triple).
    /// </summary>
    private static bool SettlementTouchesEdge(Vertex settlement, Edge edge)
    {
        var e = edge.Normalize();
        var triple = settlement.Normalize().HexCoords();
        return triple.Contains(e.HexCoord1) && triple.Contains(e.HexCoord2);
    }

    public bool CanMoveRobberTo(HexCoord newRobberHexCoord)
    {
        var hex = Hexes.FirstOrDefault(h => h.Coordinate.Equals(newRobberHexCoord));
        if (hex is null) return false;
        if (hex.Resource == ResourceCardType.Desert) return false;
        if (hex.HasRobber) return false;
        return true;
    }
}
