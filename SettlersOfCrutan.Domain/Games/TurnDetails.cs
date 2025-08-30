using SettlersOfCrutan.Domain.Core;

namespace SettlersOfCrutan.Domain.Games;
public sealed class TurnDetails
{
    public TurnStep Step { get; set; } = TurnStep.Roll;
    public TurnStatus Status { get; set; } = TurnStatus.Pending;
    public DateTimeOffset? ExpiresAt { get; set; }
    public List<DiscardHalfRequirement> DiscardHalfRequirements { get; set; } = [];

    public void Start(IDateTimeProvider clock, TimeSpan? duration)
    {
        Status = TurnStatus.Active;
        ExpiresAt = duration is null ? null : clock.UtcNow + duration.Value;
    }

    public void AdvanceTo(TurnStep step)
    {
        if (Status != TurnStatus.Active) return;
        Step = step;
    }

    public void Complete()
    {
        Status = TurnStatus.Completed;
        ExpiresAt = null;
    }

    public void MarkTimedOut()
    {
        if (Status == TurnStatus.Active)
        {
            Status = TurnStatus.TimedOut;
            ExpiresAt = null;
        }
    }

    public bool InitializeDiscardHalf(List<Player> players)
    {
        throw new NotImplementedException();
    }
}
