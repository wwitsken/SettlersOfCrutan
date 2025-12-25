import { create } from "zustand";
import type { components } from "../api/types";
import { immer } from "zustand/middleware/immer";

type Status = "idle" | "loading" | "loaded" | "error";

type State = {
  status: Status;
  error?: string | null;

  currentLobbyId: string | null;
  currentLobby: components["schemas"]["LobbyDto"] | null;
};

type Actions = {
  setLobby: (lobby: components["schemas"]["LobbyDto"]) => void;
  addLobbyMember: (playerId: string) => void;
  removeLobbyMember: (playerId: string) => void;
  changeLobbyMemberReadyStatus: (playerId: string, status: boolean) => void;
  clear: () => void;
};

export const useLobbyStore = create<State & Actions>()(
  immer((set) => ({
    status: "idle",
    error: null,
    currentLobbyId: null,
    currentLobby: null,

    setLobby: (lobby: components["schemas"]["LobbyDto"]) => {
      set({ currentLobby: lobby, currentLobbyId: lobby.lobbyId });
    },
    addLobbyMember: (playerId: string) => {
      set((state) => {
        state.currentLobby?.lobbyPlayers?.push();
      });
    },
    removeLobbyMember: (playerId) => {},
    changeLobbyMemberReadyStatus: (playerId, status) => {},
    clear: () => {},
  }))
);
