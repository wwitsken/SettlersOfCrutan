import { create } from "zustand";
import { immer } from "zustand/middleware/immer";
import { createChatSlice } from "./chatSlice";
import { createGameOverSlice } from "./gameOverSlice";
import { createGameSlice } from "./gameSlice";
import { createToastSlice } from "./toastSlice";
import type { GameStore } from "./types";

export type {
  GameChatEntry,
  GameState,
  GameActions,
  GameStore,
  GameToast,
  Status,
} from "./types";

/**
 * Consolidated per-game state: lifecycle (status/game/privateGame), chat,
 * toasts, and game-over payload. Slices are composed via the standard Zustand
 * pattern; the single cross-slice action `resetForGame` clears every
 * per-game field in one transaction so `GamePage` only has to react to a
 * `gameId` change.
 */
export const useGameStore = create<GameStore>()(
  immer((set, get, store) => ({
    ...createGameSlice(set, get, store),
    ...createChatSlice(set, get, store),
    ...createToastSlice(set, get, store),
    ...createGameOverSlice(set, get, store),

    resetForGame: (gameId) =>
      set((s) => {
        s.currentGameId = gameId;
        s.status = "loading";
        s.error = null;
        s.privateGame = null;
        s.chat = [];
        s.toasts = [];
        s.gameOver = null;
      }),
  })),
);
