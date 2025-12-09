import { create } from "zustand";
import { api } from "../api/client";
import type { components } from "../api/types";
import * as signalR from "@microsoft/signalr";

type Status = "idle" | "loading" | "loaded" | "error";

type LobbyState = {
  status: Status;
  error?: string | null;

  currentLobbyId: string | null;
  currentLobby: components["schemas"]["LobbyDto"] | null;

  hub?: signalR.HubConnection | null;

  loadLobby: (lobbyId: string) => Promise<void>;
  createLobby: () => Promise<string | null>;
  joinLobby: (lobbyId: string) => Promise<void>;
  leaveLobby: (lobbyId?: string) => Promise<void>;
  setReady: (lobbyId: string) => Promise<void>;
  setUnready: (lobbyId: string) => Promise<void>;
  clear: () => void;

  // startRealtime: (lobbyId: string) => Promise<void>;
  // stopRealtime: () => Promise<void>;
};

export const useLobbyStore = create<LobbyState>()((set, get) => {
  const setLoading = (extra?: Partial<LobbyState>) =>
    set({ status: "loading", error: null, ...extra });

  const setError = (err: unknown) =>
    set({
      status: "error",
      error: err instanceof Error ? err.message : String(err),
    });

  const loadLobbyById = async (lobbyId: string) => {
    const { data, error } = await api.GET("/api/lobby/{lobbyId}", {
      params: { path: { lobbyId } },
    });
    if (error) {
      set({ status: "error", error: String(error), currentLobby: null });
      return null;
    }
    set({ status: "loaded", currentLobby: data ?? null, error: null });
    return data ?? null;
  };

  const postAndReload = async (
    path:
      | "/api/lobby/{lobbyId}/join"
      | "/api/lobby/{lobbyId}/leave"
      | "/api/lobby/{lobbyId}/ready"
      | "/api/lobby/{lobbyId}/unready",
    lobbyId: string
  ) => {
    const { error } = await api.POST(path, { params: { path: { lobbyId } } });
    if (error) {
      setError(error);
      return false;
    }
    await loadLobbyById(lobbyId);
    return true;
  };

  return {
    status: "idle",
    error: null,
    currentLobbyId: null,
    currentLobby: null,
    hub: null,

    loadLobby: async (lobbyId: string) => {
      setLoading({ currentLobbyId: lobbyId });
      await loadLobbyById(lobbyId);
    },

    createLobby: async () => {
      setLoading();
      const { data, error } = await api.POST("/api/lobby/create");
      if (error) {
        setError(error);
        return null;
      }
      if (data) {
        set({ currentLobbyId: data });
        await get().loadLobby(data);
      }
      return data ?? null;
    },

    joinLobby: async (lobbyId: string) => {
      setLoading({ currentLobbyId: lobbyId });
      await postAndReload("/api/lobby/{lobbyId}/join", lobbyId);
    },

    leaveLobby: async (lobbyId?: string) => {
      const id = lobbyId ?? get().currentLobbyId;
      if (!id) return;
      setLoading();
      const ok = await postAndReload("/api/lobby/{lobbyId}/leave", id);
      if (!ok) return;
      set({
        status: "idle",
        currentLobby: null,
        currentLobbyId: null,
        error: null,
      });
    },

    setReady: async (lobbyId: string) => {
      setLoading();
      await postAndReload("/api/lobby/{lobbyId}/ready", lobbyId);
    },

    setUnready: async (lobbyId: string) => {
      setLoading();
      await postAndReload("/api/lobby/{lobbyId}/unready", lobbyId);
    },

    clear: () =>
      set({
        status: "idle",
        currentLobby: null,
        currentLobbyId: null,
        error: null,
        hub: null,
      }),
  };
});
