using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.Games.Resources;
using System.Text.Json.Serialization;

namespace SettlersOfCrutan.Domain.Games;

public enum PlayerColor
{
    None,
    Red,
    Blue,
    White,
    Orange,
    Green,
    Yellow,
    Brown,
    Purple
}
public record PlayerId : BaseId<string>
{
    public static PlayerId Create(string id) => new() { Value = id };
}
public class Player : Entity<PlayerId>
{
    public override PlayerId Id { get; init; }
    public string UserId { get; private set; }
    public string DisplayName { get; private set; } = string.Empty;
    public PlayerColor Color { get; private set; } = PlayerColor.None;

    // Encapsulated managers (internal for aggregate collaboration)
    [JsonInclude]
    internal ResourceHand ResourceHand { get; private set; } = null!;
    [JsonInclude]
    internal DevCardHand DevCardHand { get; private set; } = null!;
    [JsonInclude]
    internal PieceReserve PieceReserve { get; private set; } = null!;

    public DateTimeOffset? JoinedAt { get; set; } = null;

    /// <summary>Development knights successfully played (for largest army).</summary>
    [JsonInclude]
    public int KnightsPlayed { get; private set; }

    // Public read-only projections
    public int TotalResources => ResourceHand.Total;
    public int DevCardCount => DevCardHand.Total;
    public IReadOnlyDictionary<BuildableType, int> GetBuildables() => PieceReserve.Pieces;
    public IReadOnlyDictionary<ResourceCardType, int> GetResources() => ResourceHand.Cards;
    public IReadOnlyDictionary<DevelopmentCardType, int> GetDevelopmentCards() => DevCardHand.Cards;

    public static Player Create(string userId)
        => new(new() { Value = Guid.NewGuid().ToString() },
               userId,
               "",
               PlayerColor.None,
               new ResourceHand(),
               new DevCardHand(),
               PieceReserve.StandardPlayerStarting(),
               null,
               false);

    [JsonConstructor]
    private Player(PlayerId id,
                   string userId,
                   string displayName,
                   PlayerColor color,
                   ResourceHand resourceHand,
                   DevCardHand devCardHand,
                   PieceReserve pieceReserve,
                   DateTimeOffset? joinedAt,
                   bool ready,
                   int knightsPlayed = 0)
    {
        Id = id;
        UserId = userId;
        DisplayName = displayName;
        Color = color;
        ResourceHand = resourceHand;
        DevCardHand = devCardHand;
        PieceReserve = pieceReserve;
        JoinedAt = joinedAt;
        _ready = ready;
        KnightsPlayed = knightsPlayed;
    }

    public void IncrementKnightsPlayed() => KnightsPlayed++;

    // -------- Resource methods --------
    public bool CanPayResources(IEnumerable<ResourceCardAmount> cost) => ResourceHand.CanPay(cost);
    public void PayResourcesTo(ResourceHand target, IEnumerable<ResourceCardAmount> cost) => ResourceHand.PayTo(target, cost);
    public void AddResource(ResourceCardType type, int qty = 1) => ResourceHand.Add(type, qty);
    public void AddResources(IEnumerable<ResourceCardAmount> amounts)
    {
        foreach (var a in amounts) AddResource(a.Type, a.Quantity);
    }
    public void SubtractResource(ResourceCardType type, int qty = 1) => ResourceHand.Subtract(type, qty);
    public int MoveAllOfResourceTo(ResourceHand target, ResourceCardType type) => ResourceHand.MoveAllTo(target, type);
    public void TransferResource(ResourceHand target, ResourceCardType type, int qty) => ResourceHand.Transfer(target, type, qty);
    public bool HasAtLeast(ResourceCardAmount amount) => ResourceHand.HasAtLeast(amount);
    public bool HasAtLeast(IEnumerable<ResourceCardAmount> amounts) => ResourceHand.HasAtLeast(amounts);
    public int CountResource(ResourceCardType type) => ResourceHand.Count(type);

    // -------- Development card methods --------
    public bool CanUseDevCard(DevelopmentCardType type, int qty = 1) => DevCardHand.CanUse(type, qty);
    public void AddDevCard(DevelopmentCardType type, int qty = 1) => DevCardHand.Add(type, qty);
    public void UseDevCardToBank(DevCardHand bank, DevelopmentCardType type, int qty = 1) => DevCardHand.UseToBank(bank, type, qty);
    public DevelopmentCardType DrawDevCardFromBank(DevCardHand bank)
    {
        var drawn = bank.DrawRandom();
        AddDevCard(drawn, 1);
        return drawn;
    }

    // -------- Piece reserve methods --------
    public bool CanConsumePiece(BuildableType type, int qty = 1) => PieceReserve.CanConsume(type, qty);
    public void ConsumePiece(BuildableType type, int qty = 1) => PieceReserve.Consume(type, qty);
    public void ReturnPiece(BuildableType type, int qty = 1) => PieceReserve.Add(type, qty);

    // -------- Name / Color / Ready domain behavior (used by aggregate) --------
    private bool _ready;

    public bool Ready => _ready;
    public void SetName(string name) => DisplayName = name?.Trim() ?? string.Empty;
    public void SetColor(PlayerColor color) => Color = color;
    public void SetReady(bool ready) => _ready = ready;

}
