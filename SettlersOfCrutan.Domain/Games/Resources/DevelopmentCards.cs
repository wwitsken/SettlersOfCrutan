using SettlersOfCrutan.Domain.Core;
using System.Collections.Frozen;
using System.Text.Json.Serialization;

namespace SettlersOfCrutan.Domain.Games.Resources;

public record DevelopmentCardAmount(DevelopmentCardType Type, int Quantity) : ResourceAmount<DevelopmentCardType>(Type, Quantity);
public enum DevelopmentCardType
{
    Knight,         // Enable the robber and steal a resource card from another player on Build/Trade phase (sends to Steal state)
    VictoryPoint,   // Keep secret until the end of the game, worth 1 victory point
    RoadBuilding,   // Place 2 roads as if you had built them (no resource cost)
    YearOfPlenty,   // Take any 2 resource cards from the bank
    Monopoly        // Announce 1 type of resource. All other players must give you all their resource cards of that type
}
public sealed record DevCardHandId : BaseId<Guid>;

public class DevCardHand : Entity<DevCardHandId>
{
    private readonly Dictionary<DevelopmentCardType, int> _cards = [];
    public override DevCardHandId Id { get; init; }

    public IReadOnlyDictionary<DevelopmentCardType, int> Cards => _cards.ToFrozenDictionary();
    public int Total => _cards.Count == 0 ? 0 : _cards.Values.Sum();

    public DevCardHand()
    {
        Id = new DevCardHandId { Value = Guid.NewGuid() };
    }

    [JsonConstructor]
    private DevCardHand(DevCardHandId? id, IReadOnlyDictionary<DevelopmentCardType, int>? cards)
    {
        Id = id ?? new DevCardHandId { Value = Guid.NewGuid() };
        if (cards is not null)
        {
            foreach (var kv in cards) _cards[kv.Key] = kv.Value;
        }
    }

    public Result<int> Apply(DevelopmentCardType type, int delta)
    {
        _cards.TryGetValue(type, out var current);
        var next = current + delta;
        if (next < 0)
            return Result.Failure<int>(new Error("InsufficientDevCards", $"Insufficient {type} development cards."));
        if (next == 0) _cards.Remove(type); else _cards[type] = next;
        return Result.Success(next);
    }

    public void Add(DevelopmentCardType type, int quantity = 1)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(quantity, 0);
        Apply(type, quantity);
    }

    public void Subtract(DevelopmentCardType type, int quantity = 1)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(quantity, 0);
        Apply(type, -quantity);
    }

    public int Count(DevelopmentCardType type) => _cards.TryGetValue(type, out var v) ? v : 0;
    public bool HasAtLeast(DevelopmentCardType type, int quantity = 1) => Count(type) >= quantity;

    // Transactional primitives
    public bool CanUse(DevelopmentCardType type, int qty = 1) => HasAtLeast(type, qty);
    public void UseToBank(DevCardHand bank, DevelopmentCardType type, int qty = 1)
    {
        // assume CanUse true
        Subtract(type, qty);
        bank.Add(type, qty);
    }

    public DevelopmentCardType DrawRandom()
    {
        if (_cards.Count == 0) throw new InvalidOperationException("No development cards available to draw.");
        var total = Total;
        var idx = Random.Shared.Next(total);
        foreach (var kv in _cards)
        {
            if (idx < kv.Value)
            {
                Subtract(kv.Key, 1);
                return kv.Key;
            }
            idx -= kv.Value;
        }
        throw new InvalidOperationException("Failed to draw a development card.");
    }

    public static DevCardHand StandardBankDeck()
    {
        var h = new DevCardHand();
        h.Add(DevelopmentCardType.Knight, 14);
        h.Add(DevelopmentCardType.VictoryPoint, 5);
        h.Add(DevelopmentCardType.RoadBuilding, 2);
        h.Add(DevelopmentCardType.YearOfPlenty, 2);
        h.Add(DevelopmentCardType.Monopoly, 2);
        return h;
    }
}
