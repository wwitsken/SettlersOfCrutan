using SettlersOfCrutan.Domain.Core;

namespace SettlersOfCrutan.Domain.Games;

/// <summary>
/// An edge of a hex where roads can be built.
/// </summary>
/// <remarks>
/// Topology
/// - Edges are addressed by <see cref="EdgeCoord"/>, which is a pair of:
///   - The reference hex in axial coordinates (<see cref="AxialCoord"/>)
///   - The direction of the edge (<see cref="EdgeDirection"/>) in pointy-top orientation
/// - An edge has exactly two endpoint vertices, which can be derived consistently from
///   <see cref="EdgeCoord"/> via <see cref="HexTopology.GetEdgeVertices"/>. The Board caches
///   these endpoints in <see cref="VertexAId"/> and <see cref="VertexBId"/> when materialized.
/// - Similar to vertices, multiple coordinates across neighboring hexes refer to the same
///   geometric edge; always store <c>HexTopology.Canonicalize(EdgeCoord)</c>.
///
/// Lifecycle
/// - Edges are materialized lazily when a road is attempted on them. If validation fails,
///   an edge without a road can remain cached for efficient subsequent checks.
///
/// Game state
/// - At most one road may occupy an edge at a time. Road connectivity and ownership rules
///   are enforced by <see cref="Board"/> invariants.
/// </remarks>
public record EdgeId : BaseId;
public class Edge : Entity<EdgeId>
{
    public override EdgeId Id { get; init; } = new() { Value = Guid.NewGuid() };

    /// <summary>
    /// Board that owns this edge.
    /// </summary>
    public BoardId BoardId { get; set; }

    /// <summary>
    /// Canonical coordinate of this edge (reference hex + direction). Stored in canonical form.
    /// </summary>
    public EdgeCoord Coord { get; set; }

    /// <summary>
    /// First endpoint vertex id.
    /// </summary>
    public VertexId VertexAId { get; set; }

    /// <summary>
    /// Second endpoint vertex id.
    /// </summary>
    public VertexId VertexBId { get; set; }

    /// <summary>
    /// Road occupying this edge, if any.
    /// </summary>
    public RoadId? RoadId { get; set; }
}
