using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.Games.Boards.Coordinates;

namespace SettlersOfCrutan.Domain.Games.Boards;
public enum PopulationCenterLevel
{
    Settlement = 1,
    City = 2
}
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
