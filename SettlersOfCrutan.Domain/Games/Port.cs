using SettlersOfCrutan.Domain.Core;

namespace SettlersOfCrutan.Domain.Games;

public enum PortType
{
    Generic3to1,
    Brick2to1,
    Lumber2to1,
    Wool2to1,
    Grain2to1,
    Ore2to1
}

public record PortId : BaseId;
public class Port : Entity<PortId>
{
    public override PortId Id { get; } = new();

    public Port()
    {
        Id.Value = Guid.NewGuid();
    }

    public BoardId BoardId { get; set; }
    public PortType Type { get; set; }
    public EdgeCoord Edge { get; set; }
}
