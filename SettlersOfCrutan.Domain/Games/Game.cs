using SettlersOfCrutan.Domain.Core;

namespace SettlersOfCrutan.Domain.Games;

public record GameId : BaseId;
public class Game : AggregateRoot<GameId>
{
    public override GameId Id { get; init; } = new() { Value = Guid.NewGuid() };
    public string Name { get; set; }
    public List<Player> Players { get; set; } = [];

    public Board Board { get; set; }
}
