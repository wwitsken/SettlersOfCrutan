import type { GameEndedPayload } from "../../api/realtimeEvents";
import type { GameSliceCreator } from "./types";

type GameOverSlice = {
  gameOver: GameEndedPayload | null;
  setGameOver: (payload: GameEndedPayload) => void;
};

/**
 * Holds the rich `GameEnded` SignalR payload used by the end-game overlay.
 * Cleared on game change via `resetForGame`. When absent, the overlay falls
 * back to reconstructing a minimal scoreboard from `game.players`.
 */
export const createGameOverSlice: GameSliceCreator<GameOverSlice> = (set) => ({
  gameOver: null,
  setGameOver: (payload) =>
    set((s) => {
      s.gameOver = payload;
    }),
});
