using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.Games.Boards.Coordinates;

namespace SettlersOfCrutan.Domain.Games.Boards;

public enum PortType
{
    Generic3to1,
    Brick2to1,
    Lumber2to1,
    Wool2to1,
    Grain2to1,
    Ore2to1,
    None
}

public record PortId : BaseId<Edge>;
public class Port(Edge edgeCoordinate) : Entity<PortId>
{
    public override PortId Id { get; init; } = new() { Value = edgeCoordinate };
    public PortType Type { get; set; }
    public Edge EdgeCoordinate => Id.Value;
}
