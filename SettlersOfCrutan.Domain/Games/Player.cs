using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.Games.Resources;


namespace SettlersOfCrutan.Domain.Games;
public record PlayerId : BaseId<string>;
public class Player : Entity<PlayerId>
{
    public required override PlayerId Id { get; init; }
    public required string Name { get; set; }

    // Separate resource managers
    public required ResourceHand ResourceHand { get; set; }
    public required DevCardHand DevCardHand { get; set; }
    public required PieceReserve PieceReserve { get; set; }

    public DateTimeOffset? JoinedAt { get; set; } = null;

    public static Player Create(string userId)
        => new()
        {
            Id = new() { Value = userId },
            Name = userId,
            ResourceHand = new ResourceHand(),
            DevCardHand = new DevCardHand(),
            PieceReserve = PieceReserve.StandardPlayerStarting()
        };
}
