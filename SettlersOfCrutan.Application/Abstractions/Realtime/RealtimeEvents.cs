namespace SettlersOfCrutan.Application.Abstractions.Realtime;

/// <summary>
/// Wire contract for SignalR <c>eventName</c> (fourth argument to GameReceive / LobbyReceive).
/// Game play uses a single full-state payload: <see cref="GameStateUpdated"/> with per-user <see cref="SettlersOfCrutan.Application.Games.DTOs.GameDto"/>.
/// </summary>
public static class RealtimeEvents
{
    public const string UserJoinedLobby = "UserJoinedLobby";
    public const string UserLeftLobby = "UserLeftLobby";
    public const string LobbyStarted = "LobbyStarted";
    public const string UserReadyStatusChanged = "UserReadyStatusChanged";

    /// <summary>Full authoritative game snapshot for the receiving user (public + private slice).</summary>
    public const string GameStateUpdated = "GameStateUpdated";

    /// <summary>
    /// Ceremony payload fired once when a player reaches the win threshold. Payload type
    /// is <see cref="SettlersOfCrutan.Application.Games.DTOs.GameEndedDto"/>. The regular
    /// <see cref="GameStateUpdated"/> still broadcasts the transitioned snapshot after this.
    /// </summary>
    public const string GameEnded = "GameEnded";
}
