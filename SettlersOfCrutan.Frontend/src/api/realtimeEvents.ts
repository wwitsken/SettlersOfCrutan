import type { PlayerColor } from "../domain/game/gameTypes";

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
  /** In-game chat message. Payload: {@link GameChatMessagePayload}. */
  GameMessage: "gameMessage",
  /** Fired once when a player reaches the win threshold. Payload: {@link GameEndedPayload}. */
  GameEnded: "GameEnded",
} as const;

/** Shape of the payload emitted by the server for a `gameMessage` GameReceive. */
export interface GameChatMessagePayload {
  senderUserId: string;
  message: string;
}

/** Per-player final scoreboard row inside a {@link GameEndedPayload}. */
export interface FinalPlayerScorePayload {
  playerId: string;
  userId: string;
  displayName: string;
  playerColor: PlayerColor;
  /** Observable + hidden VP (authoritative total used by the overlay). */
  victoryPoints: number;
  observableVictoryPoints: number;
  hiddenVictoryPointCards: number;
  hasLongestRoad: boolean;
  hasLargestArmy: boolean;
}

/** Shape of the payload emitted by the server for a `GameEnded` GameReceive. */
export interface GameEndedPayload {
  winnerPlayerId: string;
  winnerUserId: string;
  winnerDisplayName: string;
  winnerColor: PlayerColor;
  winnerVictoryPoints: number;
  /** Full roster, pre-sorted by total VP descending. */
  finalScores: FinalPlayerScorePayload[];
}

export type RealtimeEventName =
  (typeof RealtimeEvents)[keyof typeof RealtimeEvents];

/** Lobby hub events that carry a full LobbyDto payload for every client. */
export const lobbyFullStateEvents: ReadonlySet<string> = new Set([
  RealtimeEvents.UserJoinedLobby,
  RealtimeEvents.UserLeftLobby,
  RealtimeEvents.UserReadyStatusChanged,
  RealtimeEvents.LobbyStarted,
]);
