namespace SettlersOfCrutan.Domain.Games;

public record DiscardHalfRequirement
{
    public PlayerId PlayerId { get; init; } = null!;
    public int ResourceAmount { get; set; }
}
