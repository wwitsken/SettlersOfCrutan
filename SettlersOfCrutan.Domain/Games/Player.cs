using SettlersOfCrutan.Domain.Core;


namespace SettlersOfCrutan.Domain.Games;
public record PlayerId : BaseId<string>;
public class Player : Entity<PlayerId>
{
    public required override PlayerId Id { get; init; }
    public required string Name { get; set; }
    public required ResourceBag ResourceBag { get; set; }
    public DateTimeOffset? JoinedAt { get; set; } = null;

    public static Player Create(string userId, ResourceBag resourceBag) => new()
    {
        Id = new() { Value = userId },
        ResourceBag = resourceBag,
        Name = userId.ToString()
    };
}
