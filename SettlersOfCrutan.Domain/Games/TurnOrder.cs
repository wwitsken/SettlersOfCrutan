namespace SettlersOfCrutan.Domain.Games;

public sealed class TurnOrder
{
    public IReadOnlyList<PlayerId> Players { get; }
    public GamePhase Phase { get; private set; }
    public int Index { get; private set; }
    public int Round { get; private set; }

    public TurnOrder(IEnumerable<PlayerId> players)
    {
        var list = players?.ToList() ?? throw new ArgumentNullException(nameof(players));
        if (list.Count == 0) throw new ArgumentException("Must have at least one player");
        Players = list;
        Index = 0;
        Round = 1;
        Phase = GamePhase.Setup1;
    }

    public PlayerId CurrentPlayerId() => Players[Index];

    public void Next()
    {
        if (Phase is GamePhase.Setup1)
        {
            if (Index < Players.Count - 1)
            {
                Index++;
            }
            else
            {
                Phase = GamePhase.Setup2;
                // stay on last index for snake order start
            }
            return;
        }

        if (Phase is GamePhase.Setup2)
        {
            if (Index > 0)
            {
                Index--;
            }
            else
            {
                Phase = GamePhase.Normal;
                Index = 0; // first player starts normal play
                Round = 1;
            }
            return;
        }

        // Normal clockwise
        Index = (Index + 1) % Players.Count;
        if (Index == 0) Round++;
    }
}
