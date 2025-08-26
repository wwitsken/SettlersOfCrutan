using SettlersOfCrutan.Domain.Core;

namespace SettlersOfCrutan.Domain.Games;

public record CityId : BaseId;
public class City : Entity<CityId>
{
    public override CityId Id { get; } = new();

    public City()
    {
        Id.Value = Guid.NewGuid();
    }

    public BoardId BoardId { get; set; }
    public PlayerId OwnerId { get; set; }
    public VertexId VertexId { get; set; }
}
