using SettlersOfCrutan.Domain.Core;
using System.Collections.Frozen;
using System.Text.Json.Serialization;

namespace SettlersOfCrutan.Domain.Games.Resources;
public sealed record BuildableAmount(BuildableType Type, int Quantity);
public enum BuildableType
{
    Road,
    Settlement,
    City
}
public sealed record PieceReserveId : BaseId<Guid>;

public class PieceReserve : Entity<PieceReserveId>
{
    private readonly Dictionary<BuildableType, int> _pieces = [];
    public override PieceReserveId Id { get; init; }

    public IReadOnlyDictionary<BuildableType, int> Pieces => _pieces.ToFrozenDictionary();

    public PieceReserve()
    {
        Id = new PieceReserveId { Value = Guid.NewGuid() };
    }

    [JsonConstructor]
    private PieceReserve(PieceReserveId? id, IReadOnlyDictionary<BuildableType, int>? pieces)
    {
        Id = id ?? new PieceReserveId { Value = Guid.NewGuid() };
        if (pieces is not null)
        {
            foreach (var kv in pieces) _pieces[kv.Key] = kv.Value;
        }
    }

    public Result<int> Apply(BuildableType type, int delta)
    {
        _pieces.TryGetValue(type, out var current);
        var next = current + delta;
        if (next < 0)
            return Result.Failure<int>(new Error("InsufficientPieces", $"Insufficient {type} pieces."));
        if (next == 0) _pieces.Remove(type); else _pieces[type] = next;
        return Result.Success(next);
    }

    public void Add(BuildableType type, int quantity = 1)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(quantity, 0);
        Apply(type, quantity);
    }

    public void Subtract(BuildableType type, int quantity = 1)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(quantity, 0);
        Apply(type, -quantity);
    }

    public int Count(BuildableType type) => _pieces.TryGetValue(type, out var v) ? v : 0;
    public bool HasAtLeast(BuildableType type, int quantity = 1) => Count(type) >= quantity;

    // Transactional primitives
    public bool CanConsume(BuildableType type, int qty = 1) => Count(type) >= qty;
    public void Consume(BuildableType type, int qty = 1)
    {
        // assumes CanConsume true
        Subtract(type, qty);
    }

    public static PieceReserve StandardPlayerStarting()
    {
        var r = new PieceReserve();
        r.Add(BuildableType.Settlement, 5);
        r.Add(BuildableType.City, 4);
        r.Add(BuildableType.Road, 15);
        return r;
    }
}
