import { create } from "zustand";
import type { Game } from "../domain/game/game";
import { immer } from "zustand/middleware/immer";

type Status = "idle" | "loading" | "loaded" | "error";

type State = {
  status: Status;
  error?: string | null;
  currentGameId: string | null;
};

type Actions = {
  setGame: (game: Game) => void;
  placeSettlement: () => void;
  placeCity: () => void;
  placeRoad: () => void;
  addResource: () => void;
  removeResource: () => void;
  clear: () => void;
};

export const useGamesStore = create<State & Actions>()(
  immer((set) => ({
    status: "idle",
    error: null,
    currentGameId: null,

    setGame: (game: Game) => {},
    placeSettlement: () => {},
    placeCity: () => {},
    placeRoad: () => {},
    addResource: () => {},
    removeResource: () => {},
    clear: () => {},
  })),
);
