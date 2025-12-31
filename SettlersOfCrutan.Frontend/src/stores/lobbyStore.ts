import { create } from "zustand";
import { immer } from "zustand/middleware/immer";
import type { Lobby } from "../domain/lobby/lobby";
import type { LobbyMember } from "../domain/lobby/lobbyMembers";

type Status = "idle" | "loading" | "loaded" | "error";

type State = {
  status: Status;
  error?: string;

  currentLobby?: Lobby;
};

type Actions = {
  setLobby: (lobby: Lobby) => void;
  addLobbyMember: (lobbyMember: LobbyMember) => void;
  removeLobbyMember: (lobbyMemberId: string) => void;
  changeLobbyMemberReadyStatus: (
    lobbyMemberId: string,
    status: boolean
  ) => void;
  clear: () => void;
};

export const useLobbyStore = create<State & Actions>()(
  immer((set) => ({
    status: "idle",
    error: undefined,
    currentLobby: undefined,

    setLobby: (lobby) => {
      set({ currentLobby: lobby });
    },
    addLobbyMember: (lobbyMember) => {
      set((state) => {
        state.currentLobby?.lobbyMembers?.push(lobbyMember);
      });
    },
    removeLobbyMember: (lobbyMemberId) => {
      set((state) => {
        if (!state.currentLobby || !state.currentLobby.lobbyMembers) return;
        const idx = state.currentLobby.lobbyMembers.findIndex(
          (m) => m.id === lobbyMemberId
        );
        if (idx !== -1) {
          state.currentLobby.lobbyMembers.splice(idx, 1);
        }
      });
    },
    changeLobbyMemberReadyStatus: (lobbyMemberId, status) => {
      set((state) => {
        if (!state.currentLobby || !state.currentLobby.lobbyMembers) return;
        const idx = state.currentLobby.lobbyMembers.findIndex(
          (m) => m.id === lobbyMemberId
        );
        if (idx !== -1) {
          state.currentLobby.lobbyMembers[idx].isReady = status;
        }
      });
    },
    clear: () => {},
  }))
);
