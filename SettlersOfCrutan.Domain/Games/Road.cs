using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.Games.Coordinates;

namespace SettlersOfCrutan.Domain.Games;

public record RoadId : BaseId<Edge>;
public class Road(Edge edgeCoordinate) : Entity<RoadId>
{
    public override RoadId Id { get; init; } = new() { Value = edgeCoordinate };
    public Edge EdgeCoordinate => Id.Value;
    public PlayerId OwnerId { get; set; }
}
