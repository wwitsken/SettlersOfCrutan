using SettlersOfCrutan.Domain.Core;

namespace SettlersOfCrutan.Domain.Games;

public record HexId : BaseId;
public class Hex : Entity<HexId>
{
    public override HexId Id { get; init; } = new() { Value = Guid.NewGuid() };
    public BoardId BoardId { get; set; }
    public AxialCoord Coord { get; set; }
    public ResourceType Resource { get; set; }
    public int? NumberToken { get; set; }
    public bool HasRobber { get; set; }
}
