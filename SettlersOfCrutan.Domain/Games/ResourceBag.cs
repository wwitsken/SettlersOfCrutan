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

    private List<DevelopmentCardAmount> _developmentCards = [];
    public List<DevelopmentCardAmount> DevelopmentCards
    {
        get => [.. _developmentCards];
        set => _developmentCards = value ?? [];
    }
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

    public bool HasAtLeast(ResourceType type, int amount) => Count(type) >= amount;

    public int Count(ResourceType type) => type switch
    {
        ResourceType.Brick => Brick,
        ResourceType.Lumber => Lumber,
        ResourceType.Wool => Wool,
        ResourceType.Grain => Grain,
        ResourceType.Ore => Ore,
        _ => 0
    };

    public int TotalResourceCards => Brick + Lumber + Wool + Grain + Ore;

    public bool HasDevelopmentCard(DevelopmentCardType type) => DevelopmentCards.FirstOrDefault(c => c.Type == type && c.Quantity > 0) is not null;

    public void RemoveDevelopmentCard(DevelopmentCardType type)
    {
        var card = DevelopmentCards.FirstOrDefault(c => c.Type == type && c.Quantity > 0);
        if (card != null)
        {
            if (card.Quantity == 1)
            {
                DevelopmentCards.Remove(card);
            }
            else
            {
                DevelopmentCards.Remove(card);
                DevelopmentCards.Add(card with { Quantity = card.Quantity - 1 });
            }
        }
    }

    public void AddDevelopmentCard(DevelopmentCardType type)
    {
        var card = DevelopmentCards.FirstOrDefault(c => c.Type == type);
        if (card != null)
        {
            DevelopmentCards.Remove(card);
            DevelopmentCards.Add(card with { Quantity = card.Quantity + 1 });
        }
        else
        {
            DevelopmentCards.Add(new DevelopmentCardAmount(type, 1));
        }
    }
}
