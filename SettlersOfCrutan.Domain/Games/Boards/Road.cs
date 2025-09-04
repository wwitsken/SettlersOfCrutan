using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.Games.Boards.Coordinates;

namespace SettlersOfCrutan.Domain.Games.Boards;

public record RoadId : BaseId<Edge>;
public class Road(Edge edgeCoordinate) : Entity<RoadId>
{
    public override RoadId Id { get; init; } = new() { Value = edgeCoordinate };
    public Edge EdgeCoordinate => Id.Value;
    public PlayerId OwnerId { get; set; }
}
