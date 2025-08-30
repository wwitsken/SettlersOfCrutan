using SettlersOfCrutan.Domain.Core;


namespace SettlersOfCrutan.Domain.Games;
public record PlayerId : BaseId<int>;
public class Player : Entity<PlayerId>
{
    public override PlayerId Id { get; init; }
    public string UserId { get; set; }
    public string Name { get; set; }
    public ResourceBag ResourceBag { get; set; }

    public static Player Create(int turnId, string userId) => new() { Id = new() { Value = turnId }, UserId = userId, ResourceBag = new(), Name = userId };
}
