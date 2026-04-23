import { create } from "zustand";
import type { GameEndedPayload } from "../api/realtimeEvents";

type State = {
  /** Latest ceremony payload for the current game, or null if the game hasn't ended. */
  payload: GameEndedPayload | null;
};

type Actions = {
  set: (payload: GameEndedPayload) => void;
  clear: () => void;
};

/**
 * Holds the rich `GameEnded` SignalR payload used by the end-game overlay.
 * Cleared on game change in {@link GamePage}. When absent, the overlay falls back
 * to reconstructing a minimal scoreboard from `game.players` (public VP only).
 */
export const useGameOverStore = create<State & Actions>((set) => ({
  payload: null,
  set: (payload) => set({ payload }),
  clear: () => set({ payload: null }),
}));
