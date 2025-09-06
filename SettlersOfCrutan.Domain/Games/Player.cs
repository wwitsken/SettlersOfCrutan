using SettlersOfCrutan.Domain.Core;


namespace SettlersOfCrutan.Domain.Games;
public record PlayerId : BaseId<int>;
public class Player : Entity<PlayerId>
{
    public required override PlayerId Id { get; init; }
    public required string UserId { get; set; }
    public required string Name { get; set; }
    public required ResourceBag ResourceBag { get; set; }

    public static Player Create(int turnId, string userId, ResourceBag resourceBag) => new()
    {
        Id = new() { Value = turnId },
        UserId = userId,
        ResourceBag = resourceBag,
        Name = userId.ToString()
    };
}
