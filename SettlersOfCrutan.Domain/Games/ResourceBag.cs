using SettlersOfCrutan.Domain.Core;

namespace SettlersOfCrutan.Domain.Games;
public record ResourceBagId : BaseId<Guid>;
public class ResourceBag : Entity<ResourceBagId>
{
    public override ResourceBagId Id { get; init; } = new() { Value = Guid.NewGuid() };
    public int Brick { get; set; } = 0;
    public int Lumber { get; set; } = 0;
    public int Wool { get; set; } = 0;
    public int Grain { get; set; } = 0;
    public int Ore { get; set; } = 0;
    public int Settlements { get; set; } = 0;
    public int Cities { get; set; } = 0;
    public int Roads { get; set; } = 0;
    public void SubtractResource(ResourceType type, int amount)
    {
        switch (type)
        {
            case ResourceType.Brick: Brick -= amount; break;
            case ResourceType.Lumber: Lumber -= amount; break;
            case ResourceType.Wool: Wool -= amount; break;
            case ResourceType.Grain: Grain -= amount; break;
            case ResourceType.Ore: Ore -= amount; break;
            default: break;
        }
    }
    public void AddResource(ResourceType type, int amount)
    {
        switch (type)
        {
            case ResourceType.Brick: Brick += amount; break;
            case ResourceType.Lumber: Lumber += amount; break;
            case ResourceType.Wool: Wool += amount; break;
            case ResourceType.Grain: Grain += amount; break;
            case ResourceType.Ore: Ore += amount; break;
            default: break;
        }
    }

    public bool HasAtLeast(ResourceType type, int amount) => type switch
    {
        ResourceType.Brick => Brick >= amount,
        ResourceType.Lumber => Lumber >= amount,
        ResourceType.Wool => Wool >= amount,
        ResourceType.Grain => Grain >= amount,
        ResourceType.Ore => Ore >= amount,
        _ => false
    };
    public int TotalResourceCards => Brick + Lumber + Wool + Grain + Ore;
}
