import { create } from "zustand";
import { subscribeWithSelector } from "zustand/middleware";
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

  startRealtime: (lobbyId: string) => Promise<void>;
  stopRealtime: () => Promise<void>;
};

export const useLobbyStore = create<LobbyState>()(
  subscribeWithSelector((set, get) => ({
    status: "idle",
    error: null,
    currentLobbyId: null,
    currentLobby: null,
    hub: null,

    loadLobby: async (lobbyId: string) => {
      set({ status: "loading", error: null, currentLobbyId: lobbyId });
      const { data, error } = await api.GET("/api/lobby/{lobbyId}", {
        params: { path: { lobbyId } },
      });
      if (error) {
        set({ status: "error", error: String(error), currentLobby: null });
        return;
      }
      set({ status: "loaded", currentLobby: data ?? null, error: null });
    },

    createLobby: async () => {
      set({ status: "loading", error: null });
      const { data, error } = await api.POST("/api/lobby/create");
      if (error) {
        set({ status: "error", error: String(error) });
        return null;
      }
      if (data) {
        set({ currentLobbyId: data });
        await get().loadLobby(data);
      }
      return data ?? null;
    },

    joinLobby: async (lobbyId: string) => {
      set({ status: "loading", error: null, currentLobbyId: lobbyId });
      const { error } = await api.POST("/api/lobby/{lobbyId}/join", {
        params: { path: { lobbyId } },
      });
      if (error) {
        set({ status: "error", error: String(error) });
        return;
      }
      await get().loadLobby(lobbyId);
    },

    leaveLobby: async (lobbyId?: string) => {
      const id = lobbyId ?? get().currentLobbyId;
      if (!id) return;
      set({ status: "loading", error: null });
      const { error } = await api.POST("/api/lobby/{lobbyId}/leave", {
        params: { path: { lobbyId: id } },
      });
      if (error) {
        set({ status: "error", error: String(error) });
        return;
      }
      set({
        status: "idle",
        currentLobby: null,
        currentLobbyId: null,
        error: null,
      });
    },

    setReady: async (lobbyId: string) => {
      set({ status: "loading", error: null });
      const { error } = await api.POST("/api/lobby/{lobbyId}/ready", {
        params: { path: { lobbyId } },
      });
      if (error) {
        set({ status: "error", error: String(error) });
        return;
      }
      await get().loadLobby(lobbyId);
    },

    setUnready: async (lobbyId: string) => {
      set({ status: "loading", error: null });
      const { error } = await api.POST("/api/lobby/{lobbyId}/unready", {
        params: { path: { lobbyId } },
      });
      if (error) {
        set({ status: "error", error: String(error) });
        return;
      }
      await get().loadLobby(lobbyId);
    },

    clear: () =>
      set({
        status: "idle",
        currentLobby: null,
        currentLobbyId: null,
        error: null,
        hub: null,
      }),

    startRealtime: async (lobbyId: string) => {
      // Reuse running connection if present
      let hub = get().hub;
      if (!hub) {
        hub = new signalR.HubConnectionBuilder()
          .withUrl("/api/realtime-hub", { withCredentials: true })
          .withAutomaticReconnect()
          .build();

        // When reconnected or server notifies, refresh lobby state
        hub.onreconnected(async () => {
          const id = get().currentLobbyId ?? lobbyId;
          if (id) await get().loadLobby(id);
        });

        // If backend emits a generic lobby update event
        hub.on("LobbyUpdated", async () => {
          const id = get().currentLobbyId ?? lobbyId;
          if (id) await get().loadLobby(id);
        });

        set({ hub });
      }

      if (hub.state === signalR.HubConnectionState.Disconnected) {
        try {
          await hub.start();
        } catch (e) {
          set({ error: e instanceof Error ? e.message : String(e) });
        }
      }
    },

    stopRealtime: async () => {
      const hub = get().hub;
      if (hub && hub.state !== signalR.HubConnectionState.Disconnected) {
        try {
          await hub.stop();
        } finally {
          set({ hub: null });
        }
      }
    },
  }))
);
