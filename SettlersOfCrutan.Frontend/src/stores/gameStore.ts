import { create } from "zustand";
import type { Game } from "../domain/game/game";
import type { PrivateGameInfo } from "../domain/game/privateGame";
import { immer } from "zustand/middleware/immer";

type Status = "idle" | "loading" | "loaded" | "error";

type State = {
  status: Status;
  error?: string | null;
  currentGameId: string | null;
  game: Game | null;
  /** Filled when the last applied payload included `myPrivateGameInfo` (may be null). */
  privateGame: PrivateGameInfo | null;
};

type Actions = {
  /**
   * Apply a loaded server snapshot. Always pass `privateGame` explicitly (`null` if absent).
   */
  applyLoadedState: (game: Game, privateGame: PrivateGameInfo | null) => void;
  setLoading: (gameId: string) => void;
  setError: (message: string) => void;
  clear: () => void;
};

export const useGamesStore = create<State & Actions>()(
  immer((set) => ({
    status: "idle",
    error: null,
    currentGameId: null,
    game: null,
    privateGame: null,

    applyLoadedState: (game: Game, privateGame: PrivateGameInfo | null) =>
      set((s) => {
        s.game = game;
        s.currentGameId = game.id;
        s.status = "loaded";
        s.error = null;
        s.privateGame = privateGame;
      }),
    setLoading: (gameId: string) =>
      set((s) => {
        s.currentGameId = gameId;
        s.status = "loading";
        s.error = null;
        s.privateGame = null;
      }),
    setError: (message: string) =>
      set((s) => {
        s.status = "error";
        s.error = message;
      }),
    clear: () =>
      set((s) => {
        s.game = null;
        s.privateGame = null;
        s.currentGameId = null;
        s.status = "idle";
        s.error = null;
      }),
  })),
);
