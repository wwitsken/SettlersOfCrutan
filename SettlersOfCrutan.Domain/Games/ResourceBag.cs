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
        private set => _developmentCards = value ?? [];
    }
    public void SubtractResource(ResourceType type, int amount)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(amount, 0, nameof(amount));
        ResourceAmount resourceAmount = new(type, -amount);
        MapToResourceTransaction(type)(this, resourceAmount);
    }
    public void AddResource(ResourceType type, int amount)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(amount, 0, nameof(amount));
        ResourceAmount resourceAmount = new(type, amount);
        MapToResourceTransaction(type)(this, resourceAmount);
    }

    public void ApplyResourceAmounts(IEnumerable<ResourceAmount> resourceAmounts)
    {
        foreach (var resourceAmount in resourceAmounts) MapToResourceTransaction(resourceAmount.Type)(this, resourceAmount);
    }

    public bool HasAtLeast(ResourceAmount resourceAmount) => Count(resourceAmount.Type) >= resourceAmount.Quantity;
    public bool HasAtLeast(ResourceType type, int amount) => Count(type) >= amount;
    public bool HasAtLeast(IEnumerable<ResourceAmount> resourceAmounts) => resourceAmounts.All(HasAtLeast);
    public int TotalResourceCards => Enum.GetValues<ResourceType>().Sum(Count);

    private static Action<ResourceBag, ResourceAmount> MapToResourceTransaction(ResourceType resourceType) =>
        resourceType switch
        {
            ResourceType.Brick => (rb, ra) => rb.Brick += ra.Quantity,
            ResourceType.Lumber => (rb, ra) => rb.Lumber += ra.Quantity,
            ResourceType.Wool => (rb, ra) => rb.Wool += ra.Quantity,
            ResourceType.Grain => (rb, ra) => rb.Grain += ra.Quantity,
            ResourceType.Ore => (rb, ra) => rb.Ore += ra.Quantity,
            _ => throw new NotImplementedException($"Transaction type {resourceType} is not implemented.")
        };

    public int Count(ResourceType type) => type switch
    {
        ResourceType.Brick => Brick,
        ResourceType.Lumber => Lumber,
        ResourceType.Wool => Wool,
        ResourceType.Grain => Grain,
        ResourceType.Ore => Ore,
        _ => 0
    };


    public bool HasDevelopmentCard(DevelopmentCardType type) => DevelopmentCards.FirstOrDefault(c => c.Type == type && c.Quantity > 0) is not null;

    public DevelopmentCardType DrawRandomDevelopmentCard()
    {
        if (DevelopmentCards.Count == 0) throw new InvalidOperationException("No development cards available to draw.");
        var totalCards = DevelopmentCards.Sum(c => c.Quantity);
        var randomIndex = Random.Shared.Next(totalCards);
        foreach (var card in DevelopmentCards)
        {
            if (randomIndex < card.Quantity)
            {
                RemoveSingleDevelopmentCard(card.Type);
                return card.Type;
            }
            randomIndex -= card.Quantity;
        }
        // This should never be reached
        throw new InvalidOperationException("Failed to draw a development card.");
    }

    public void RemoveSingleDevelopmentCard(DevelopmentCardType type)
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

    public void AddSingleDevelopmentCard(DevelopmentCardType type)
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

    public static ResourceBag StandardBank()
    {
        var bag = new ResourceBag()
        {
            Brick = 19,
            Lumber = 19,
            Wool = 19,
            Grain = 19,
            Ore = 19,
        };
        bag.DevelopmentCards.Add(new DevelopmentCardAmount(DevelopmentCardType.Knight, 14));
        bag.DevelopmentCards.Add(new DevelopmentCardAmount(DevelopmentCardType.VictoryPoint, 5));
        bag.DevelopmentCards.Add(new DevelopmentCardAmount(DevelopmentCardType.RoadBuilding, 2));
        bag.DevelopmentCards.Add(new DevelopmentCardAmount(DevelopmentCardType.YearOfPlenty, 2));
        bag.DevelopmentCards.Add(new DevelopmentCardAmount(DevelopmentCardType.Monopoly, 2));
        return bag;
    }

    public static ResourceBag StandardPlayerStartingBag()
    {
        return new ResourceBag()
        {
            Brick = 0,
            Lumber = 0,
            Wool = 0,
            Grain = 0,
            Ore = 0,
            Settlements = 5,
            Cities = 4,
            Roads = 15,
            DevelopmentCards = []
        };
    }
}
