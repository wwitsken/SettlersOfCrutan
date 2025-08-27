using SettlersOfCrutan.Domain.Core;

namespace SettlersOfCrutan.Domain.Games;

public record SettlementId : BaseId;
public class Settlement : Entity<SettlementId>
{
    public override SettlementId Id { get; init; } = new() { Value = Guid.NewGuid() };
    public BoardId BoardId { get; set; }
    public PlayerId OwnerId { get; set; }
    public VertexId VertexId { get; set; }
}
