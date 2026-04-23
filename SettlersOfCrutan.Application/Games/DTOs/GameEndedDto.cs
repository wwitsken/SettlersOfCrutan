using SettlersOfCrutan.Domain.Games;

namespace SettlersOfCrutan.Application.Games.DTOs;

/// <summary>Per-player final scoreboard row broadcast in <see cref="GameEndedDto"/>.</summary>
public sealed record FinalPlayerScoreDto(
    string PlayerId,
    Guid UserId,
    string DisplayName,
    PlayerColor PlayerColor,
    int VictoryPoints,
    int ObservableVictoryPoints,
    int HiddenVictoryPointCards,
    bool HasLongestRoad,
    bool HasLargestArmy);

/// <summary>
/// Broadcast once over SignalR (<see cref="SettlersOfCrutan.Application.Abstractions.Realtime.RealtimeEvents.GameEnded"/>)
/// when a player reaches the win threshold. Carries the winner plus the full final score breakdown
/// (including hidden VP cards) for the end-game overlay.
/// </summary>
public sealed record GameEndedDto(
    string WinnerPlayerId,
    Guid WinnerUserId,
    string WinnerDisplayName,
    PlayerColor WinnerColor,
    int WinnerVictoryPoints,
    IReadOnlyList<FinalPlayerScoreDto> FinalScores);
