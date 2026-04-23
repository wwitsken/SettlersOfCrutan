using SettlersOfCrutan.Domain.Games;
using SettlersOfCrutan.Domain.Games.Resources;

namespace SettlersOfCrutan.Application.Games;

/// <summary>
/// Detects the first player to reach the win threshold (observable + hidden VP) and transitions
/// the aggregate to <see cref="GamePhase.GameEnd"/> via <see cref="Game.DeclareWinner(PlayerId)"/>.
/// Reuses <see cref="GamePresentationScoring"/> so VP math stays single-sourced.
/// Deterministic tie-break: earlier play order wins (rare — two players can't normally reach 11
/// on the same state mutation).
/// </summary>
public static class WinConditionEvaluator
{
    public const int WinVictoryPoints = 11;

    /// <summary>
    /// If any player is at or above the win threshold, transitions the game to GameEnd and
    /// raises <see cref="DomainEvents.PlayerWonDomainEvent"/>. No-op once ended.
    /// </summary>
    public static void EvaluateAndTransition(Game game)
    {
        ArgumentNullException.ThrowIfNull(game);
        if (game.GamePhase == GamePhase.GameEnd) return;

        var buildingVp = GamePresentationScoring.BuildingVictoryPoints(game);
        var longestRoad = GamePresentationScoring.LongestRoadHolders(game);
        var largestArmy = GamePresentationScoring.LargestArmyHolders(game);

        foreach (var player in game.Players)
        {
            if (TotalVictoryPoints(player, buildingVp, longestRoad, largestArmy) >= WinVictoryPoints)
            {
                game.DeclareWinner(player.Id);
                return;
            }
        }
    }

    /// <summary>Observable VP + hidden VP development cards for a single player.</summary>
    public static int TotalVictoryPoints(
        Player player,
        IReadOnlyDictionary<PlayerId, int> buildingVp,
        IReadOnlySet<PlayerId> longestRoad,
        IReadOnlySet<PlayerId> largestArmy)
    {
        ArgumentNullException.ThrowIfNull(player);
        ArgumentNullException.ThrowIfNull(buildingVp);
        ArgumentNullException.ThrowIfNull(longestRoad);
        ArgumentNullException.ThrowIfNull(largestArmy);

        var observable = GamePresentationScoring.ObservableVictoryPoints(
            buildingVp.GetValueOrDefault(player.Id, 0),
            longestRoad.Contains(player.Id),
            largestArmy.Contains(player.Id));
        var hidden = player.GetDevelopmentCards().TryGetValue(DevelopmentCardType.VictoryPoint, out var n) ? n : 0;
        return observable + hidden;
    }
}
