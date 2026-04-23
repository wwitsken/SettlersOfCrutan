import type { StateCreator } from "zustand";
import type { GameEndedPayload } from "../../api/realtimeEvents";
import type { Game } from "../../domain/game/game";
import type { PrivateGameInfo } from "../../domain/game/privateGame";

export type Status = "idle" | "loading" | "loaded" | "error";

/** One entry in the in-memory game chat log. */
export type GameChatEntry = {
  id: string;
  senderUserId: string;
  message: string;
  /** Server-issued UTC timestamp as epoch millis. */
  timestamp: number;
};

export type GameToast = {
  id: string;
  message: string;
};

export type GameState = {
  status: Status;
  error: string | null;
  currentGameId: string | null;
  game: Game | null;
  /** Filled when the last applied payload included `myPrivateGameInfo` (may be null). */
  privateGame: PrivateGameInfo | null;
  chat: GameChatEntry[];
  toasts: GameToast[];
  gameOver: GameEndedPayload | null;
};

export type GameActions = {
  /**
   * Apply a loaded server snapshot. Always pass `privateGame` explicitly
   * (`null` if absent).
   */
  applyLoadedState: (game: Game, privateGame: PrivateGameInfo | null) => void;
  setLoading: (gameId: string) => void;
  setError: (message: string) => void;
  appendChat: (entry: Omit<GameChatEntry, "id">) => void;
  pushToast: (message: string) => void;
  dismissToast: (id: string) => void;
  setGameOver: (payload: GameEndedPayload) => void;
  /**
   * Reset every per-game slice for a new game id. Sets `currentGameId`, moves
   * status to `"loading"`, and clears chat, toasts, game-over, private-slice,
   * and error fields. Leaves `game` untouched so callers may re-apply a
   * freshly loaded payload over the top.
   */
  resetForGame: (gameId: string) => void;
};

export type GameStore = GameState & GameActions;

/**
 * Slice factory type for this store: immer-backed creators that contribute a
 * subset `T` of the combined {@link GameStore} surface.
 */
export type GameSliceCreator<T> = StateCreator<
  GameStore,
  [["zustand/immer", never]],
  [],
  T
>;
