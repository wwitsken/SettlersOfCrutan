using SettlersOfCrutan.Domain.Core;

namespace SettlersOfCrutan.Domain.Games;
public record PlayerId : BaseId;
public class Player : Entity<PlayerId>
{
    public override PlayerId Id { get; } = new();

    public Player()
    {
        Id.Value = Guid.NewGuid();
    }

    public GameId GameId { get; set; }
    public string UserId { get; set; }
    public string Name { get; set; }
    public int PlayerOrder { get; set; }
}
