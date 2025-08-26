using SettlersOfCrutan.Domain.Core;

namespace SettlersOfCrutan.Domain.Games;

public record GameId : BaseId;
public class Game : AggregateRoot<GameId>
{
    public override GameId Id { get; } = new();

    public Game()
    {
        Id.Value = Guid.NewGuid();
    }

    public string Name { get; set; }
    public List<Player> Players { get; set; } = [];

}
