import type { Game } from "../../domain/game/game";
import type { PrivateGameInfo } from "../../domain/game/privateGame";
import type { GameSliceCreator, Status } from "./types";

type GameLifecycleSlice = {
  status: Status;
  error: string | null;
  currentGameId: string | null;
  game: Game | null;
  privateGame: PrivateGameInfo | null;
  applyLoadedState: (game: Game, privateGame: PrivateGameInfo | null) => void;
  setLoading: (gameId: string) => void;
  setError: (message: string) => void;
};

export const createGameSlice: GameSliceCreator<GameLifecycleSlice> = (set) => ({
  status: "idle",
  error: null,
  currentGameId: null,
  game: null,
  privateGame: null,

  applyLoadedState: (game, privateGame) =>
    set((s) => {
      s.game = game;
      s.currentGameId = game.id;
      s.status = "loaded";
      s.error = null;
      s.privateGame = privateGame;
    }),

  setLoading: (gameId) =>
    set((s) => {
      s.currentGameId = gameId;
      s.status = "loading";
      s.error = null;
      s.privateGame = null;
    }),

  setError: (message) =>
    set((s) => {
      s.status = "error";
      s.error = message;
    }),
});
