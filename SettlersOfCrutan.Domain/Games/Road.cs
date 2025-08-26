using SettlersOfCrutan.Domain.Core;

namespace SettlersOfCrutan.Domain.Games;

public record RoadId : BaseId;
public class Road : Entity<RoadId>
{
    public override RoadId Id { get; } = new();

    public Road()
    {
        Id.Value = Guid.NewGuid();
    }

    public BoardId BoardId { get; set; }
    public PlayerId OwnerId { get; set; }
    public EdgeId EdgeId { get; set; }
}
