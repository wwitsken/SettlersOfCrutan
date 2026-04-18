/**
 * Mirrors Application Abstractions RealtimeEvents — fourth arg to GameReceive / LobbyReceive.
 * Game play uses GameStateUpdated with per-user GameDto (nested `game` + `myPrivateGameInfo`).
 */
export const RealtimeEvents = {
  UserJoinedLobby: "UserJoinedLobby",
  UserLeftLobby: "UserLeftLobby",
  LobbyStarted: "LobbyStarted",
  UserReadyStatusChanged: "UserReadyStatusChanged",
  GameStateUpdated: "GameStateUpdated",
} as const;

export type RealtimeEventName =
  (typeof RealtimeEvents)[keyof typeof RealtimeEvents];

/** Lobby hub events that carry a full LobbyDto payload for every client. */
export const lobbyFullStateEvents: ReadonlySet<string> = new Set([
  RealtimeEvents.UserJoinedLobby,
  RealtimeEvents.UserLeftLobby,
  RealtimeEvents.UserReadyStatusChanged,
  RealtimeEvents.LobbyStarted,
]);
