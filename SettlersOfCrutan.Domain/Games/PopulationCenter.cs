using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.Games.Coordinates;

namespace SettlersOfCrutan.Domain.Games;
public record PopulationCenterId : BaseId<Vertex>;
public class PopulationCenter(Vertex vertexCoordinate) : Entity<PopulationCenterId>
{
    public override PopulationCenterId Id { get; init; } = new() { Value = vertexCoordinate };
    public Vertex VertexCoordinate => Id.Value;
    public PlayerId PlayerOwner { get; set; }
    public PopulationCenterLevel Level { get; set; } = PopulationCenterLevel.Settlement;
    public static PopulationCenter CreateSettlement(Vertex vertexCoord, PlayerId playerId) =>
        new(vertexCoord)
        {
            PlayerOwner = playerId,
            Level = PopulationCenterLevel.Settlement
        };
}
