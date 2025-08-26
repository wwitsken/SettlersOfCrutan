using SettlersOfCrutan.Domain.Core;

namespace SettlersOfCrutan.Domain.Games;

/// <summary>
/// A vertex in the hex grid where up to three hexes meet (a settlement/city location).
/// </summary>
/// <remarks>
/// Topology
/// - Vertices are addressed by <see cref="VertexCoord"/>, which is a pair of:
///   - The reference hex in axial coordinates (<see cref="AxialCoord"/>)
///   - The corner of that hex (<see cref="VertexCorner"/>) in pointy-top orientation
/// - Two distinct <see cref="VertexCoord"/> values can represent the same geometric vertex
///   (e.g., the TopRight corner of one hex is the TopLeft of its NE neighbor). This model
///   treats the coordinate on the reference hex as canonical. Edges and helpers use
///   <see cref="HexTopology"/> to translate consistently. Always store
///   <c>HexTopology.Canonicalize(VertexCoord)</c>.
///
/// Lifecycle
/// - Vertices are materialized lazily by operations (e.g., when placing a settlement or
///   when building a road that touches the vertex). A vertex can exist without any
///   settlement/city assigned.
///
/// Game state
/// - At most one settlement or city can occupy a vertex. The distance rule is enforced
///   when placing settlements by the <see cref="Board"/> invariants.
/// </remarks>
public record VertexId : BaseId;
public class Vertex : Entity<VertexId>
{
    public override VertexId Id { get; } = new();

    public Vertex()
    {
        Id.Value = Guid.NewGuid();
    }

    /// <summary>
    /// Board that owns this vertex.
    /// </summary>
    public BoardId BoardId { get; set; }

    /// <summary>
    /// Canonical coordinate of this vertex (reference hex + corner). Stored in canonical form.
    /// </summary>
    public VertexCoord Coord { get; set; }

    /// <summary>
    /// Settlement occupying this vertex, if any.
    /// </summary>
    public SettlementId? SettlementId { get; set; }

    /// <summary>
    /// City occupying this vertex, if any.
    /// </summary>
    public CityId? CityId { get; set; }
}
