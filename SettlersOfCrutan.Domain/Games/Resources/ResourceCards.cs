using SettlersOfCrutan.Domain.Core;
using System.Collections.Frozen;
using System.Text.Json.Serialization;

namespace SettlersOfCrutan.Domain.Games.Resources;

public record ResourceCardAmount(ResourceCardType Type, int Quantity) : ResourceAmount<ResourceCardType>(Type, Quantity);
public enum ResourceCardType
{
    None = 0,
    Brick,
    Lumber,
    Wool,
    Grain,
    Ore,
    Desert,
    Water
}

public sealed record ResourceHandId : BaseId<Guid>;

public class ResourceHand : Entity<ResourceHandId>
{
    private readonly Dictionary<ResourceCardType, int> _cards = [];
    public override ResourceHandId Id { get; init; }

    public IReadOnlyDictionary<ResourceCardType, int> Cards => _cards.ToFrozenDictionary();
    public int Total => _cards.Count == 0 ? 0 : _cards.Values.Sum();

    public ResourceHand()
    {
        Id = new ResourceHandId { Value = Guid.NewGuid() };
    }

    [JsonConstructor]
    private ResourceHand(ResourceHandId? id, IReadOnlyDictionary<ResourceCardType, int>? cards)
    {
        Id = id ?? new ResourceHandId { Value = Guid.NewGuid() };
        if (cards is not null)
        {
            foreach (var kv in cards) _cards[kv.Key] = kv.Value;
        }
    }

    public Result<int> Apply(ResourceCardType type, int delta)
    {
        _cards.TryGetValue(type, out var current);
        var next = current + delta;
        if (next < 0)
            return Result.Failure<int>(new Error("InsufficientResources", $"Insufficient {type} resources."));
        if (next == 0) _cards.Remove(type); else _cards[type] = next;
        return Result.Success(next);
    }

    public void Add(ResourceCardType type, int quantity = 1)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(quantity, 0);
        Apply(type, quantity);
    }

    public void Subtract(ResourceCardType type, int quantity = 1)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(quantity, 0);
        Apply(type, -quantity);
    }

    public int Count(ResourceCardType type) => _cards.TryGetValue(type, out var v) ? v : 0;

    public bool HasAtLeast(ResourceCardType type, int quantity = 1) => Count(type) >= quantity;
    public bool HasAtLeast(ResourceCardAmount amount) => HasAtLeast(amount.Type, amount.Quantity);
    public bool HasAtLeast(IEnumerable<ResourceCardAmount> amounts) => amounts.All(HasAtLeast);

    public Result<Nothing> ApplyResourceAmounts(IEnumerable<ResourceCardAmount> resourceAmounts)
    {
        // pre-validate
        foreach (var ra in resourceAmounts)
        {
            _cards.TryGetValue(ra.Type, out var cur);
            if (cur + ra.Quantity < 0)
                return Result.Failure<Nothing>(new Error("InsufficientResources", $"Insufficient {ra.Type} resources."));
        }
        foreach (var ra in resourceAmounts)
            Apply(ra.Type, ra.Quantity);
        return Result.Success();
    }

    // --- Transactional primitives ---
    public bool CanPay(IEnumerable<ResourceCardAmount> cost)
    {
        foreach (var c in cost)
        {
            if (!HasAtLeast(c)) return false;
        }
        return true;
    }

    public void PayTo(ResourceHand target, IEnumerable<ResourceCardAmount> cost)
    {
        // assumes CanPay true; no validation here
        foreach (var c in cost)
        {
            Subtract(c.Type, c.Quantity);
            target.Add(c.Type, c.Quantity);
        }
    }

    // Move all of a given resource type to another hand; returns quantity moved
    public int MoveAllTo(ResourceHand target, ResourceCardType type)
    {
        var qty = Count(type);
        if (qty <= 0) return 0;
        Subtract(type, qty);
        target.Add(type, qty);
        return qty;
    }

    // Transfer a fixed quantity (assumes caller validated availability)
    public void Transfer(ResourceHand target, ResourceCardType type, int qty)
    {
        Subtract(type, qty);
        target.Add(type, qty);
    }

    public static ResourceHand StandardBankResources()
    {
        var h = new ResourceHand();
        h.Add(ResourceCardType.Brick, 19);
        h.Add(ResourceCardType.Lumber, 19);
        h.Add(ResourceCardType.Wool, 19);
        h.Add(ResourceCardType.Grain, 19);
        h.Add(ResourceCardType.Ore, 19);
        return h;
    }
}
