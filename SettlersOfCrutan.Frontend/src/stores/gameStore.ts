import { create } from "zustand";
import type { Game } from "../domain/game/game";
import { immer } from "zustand/middleware/immer";

type Status = "idle" | "loading" | "loaded" | "error";

type State = {
  status: Status;
  error?: string | null;
  currentGameId: string | null;
  game: Game | null;
};

type Actions = {
  setGame: (game: Game) => void;
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

    setGame: (game: Game) =>
      set((s) => {
        s.game = game;
        s.currentGameId = game.id;
        s.status = "loaded";
        s.error = null;
      }),
    setLoading: (gameId: string) =>
      set((s) => {
        s.currentGameId = gameId;
        s.status = "loading";
        s.error = null;
      }),
    setError: (message: string) =>
      set((s) => {
        s.status = "error";
        s.error = message;
      }),
    clear: () =>
      set((s) => {
        s.game = null;
        s.currentGameId = null;
        s.status = "idle";
        s.error = null;
      }),
  })),
);
