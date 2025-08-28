using SettlersOfCrutan.Domain.Core;

namespace SettlersOfCrutan.Domain.Games;
public record PlayerId : BaseId<Guid>;
public class Player : Entity<PlayerId>
{
    public override PlayerId Id { get; init; } = new() { Value = Guid.NewGuid() };
    public string UserId { get; set; }
    public string Name { get; set; }
    public int PlayerOrder { get; set; }
}
