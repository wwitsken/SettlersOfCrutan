import { create } from "zustand";
import { subscribeWithSelector } from "zustand/middleware";
import { api } from "../api/client";
import type { components } from "../api/types";

type Status = "idle" | "loading" | "loaded" | "error";

type GamesState = {
  status: Status;
  error?: string | null;

  currentGameId: string | null;

  createGame: (
    req: components["schemas"]["CreateGameRequest"]
  ) => Promise<string | null>;
  joinGame: (
    id: string,
    req: components["schemas"]["JoinGameRequest"]
  ) => Promise<void>;
  loadGame: (id: string) => Promise<void>;

  rollDice: (
    id: string
  ) => Promise<components["schemas"]["RollDiceCommandResult"] | null>;
  placeInitial: (
    id: string,
    req: components["schemas"]["BuildInitialRequest"]
  ) => Promise<void>;
  buildRoad: (
    id: string,
    req: components["schemas"]["BuildRoadRequest"]
  ) => Promise<void>;
  buildSettlement: (
    id: string,
    req: components["schemas"]["BuildSettlementRequest"]
  ) => Promise<void>;
  buildCity: (
    id: string,
    req: components["schemas"]["UpgradeSettlementToCityRequest"]
  ) => Promise<void>;

  offerTrade: (
    id: string,
    req: components["schemas"]["OfferTradeRequest"]
  ) => Promise<void>;
  acceptTrade: (
    id: string,
    req: components["schemas"]["AcceptTradeRequest"]
  ) => Promise<void>;
  maritimeTrade4to1: (
    id: string,
    req: components["schemas"]["MaritimeTradeRequest"]
  ) => Promise<void>;
  maritimeTrade3to1: (
    id: string,
    req: components["schemas"]["MaritimeTradeRequest"]
  ) => Promise<void>;
  maritimeTrade2to1: (
    id: string,
    req: components["schemas"]["MaritimeTradeRequest"]
  ) => Promise<void>;

  useRoadBuilding: (
    id: string,
    req: components["schemas"]["UseRoadBuildingRequest"]
  ) => Promise<void>;
  useMonopoly: (
    id: string,
    req: components["schemas"]["UseMonopolyRequest"]
  ) => Promise<number | null>;
  useYearOfPlenty: (
    id: string,
    req: components["schemas"]["UseYearOfPlentyRequest"]
  ) => Promise<components["schemas"]["UseYearOfPlentyCommandResult"] | null>;
  useKnight: (
    id: string,
    req: components["schemas"]["UseKnightRequest"]
  ) => Promise<void>;

  endTurn: (id: string) => Promise<string | null>;
  resolveRobber: (
    id: string,
    req: components["schemas"]["ResolveRobberRequest"]
  ) => Promise<components["schemas"]["ResourceCardType"] | null>;
  discardHalf: (
    id: string,
    req: components["schemas"]["DiscardHalfRequest"]
  ) => Promise<void>;
};

export const useGamesStore = create<GamesState>()(
  subscribeWithSelector((set) => ({
    status: "idle",
    error: null,
    currentGameId: null,

    createGame: async (req) => {
      set({ status: "loading", error: null });
      const { data, error } = await api.POST("/api/games/create", {
        body: req,
      });
      if (error) {
        set({ status: "error", error: String(error) });
        return null;
      }
      if (data) set({ currentGameId: data });
      set({ status: "loaded" });
      return data ?? null;
    },

    joinGame: async (id, req) => {
      set({ status: "loading", error: null, currentGameId: id });
      const { error } = await api.POST("/api/games/{id}/join", {
        params: { path: { id } },
        body: req,
      });
      if (error) {
        set({ status: "error", error: String(error) });
        return;
      }
      set({ status: "loaded" });
    },

    loadGame: async (id) => {
      set({ status: "loading", error: null, currentGameId: id });
      const { error } = await api.GET("/api/games/{id}", {
        params: { path: { id } },
      });
      if (error) {
        set({ status: "error", error: String(error) });
        return;
      }
      set({ status: "loaded" });
      return;
    },

    rollDice: async (id) => {
      set({ status: "loading", error: null });
      const { data, error } = await api.POST("/api/games/{id}/play/roll-dice", {
        params: { path: { id } },
      });
      if (error) {
        set({ status: "error", error: String(error) });
        return null;
      }
      set({ status: "loaded" });
      return data ?? null;
    },

    placeInitial: async (id, req) => {
      set({ status: "loading", error: null });
      const { error } = await api.POST("/api/games/{id}/play/place-initial", {
        params: { path: { id } },
        body: req,
      });
      set(
        error ? { status: "error", error: String(error) } : { status: "loaded" }
      );
    },

    buildRoad: async (id, req) => {
      set({ status: "loading", error: null });
      const { error } = await api.POST("/api/games/{id}/build/road", {
        params: { path: { id } },
        body: req,
      });
      set(
        error ? { status: "error", error: String(error) } : { status: "loaded" }
      );
    },

    buildSettlement: async (id, req) => {
      set({ status: "loading", error: null });
      const { error } = await api.POST("/api/games/{id}/build/settlement", {
        params: { path: { id } },
        body: req,
      });
      set(
        error ? { status: "error", error: String(error) } : { status: "loaded" }
      );
    },

    buildCity: async (id, req) => {
      set({ status: "loading", error: null });
      const { error } = await api.POST("/api/games/{id}/build/city", {
        params: { path: { id } },
        body: req,
      });
      set(
        error ? { status: "error", error: String(error) } : { status: "loaded" }
      );
    },

    offerTrade: async (id, req) => {
      set({ status: "loading", error: null });
      const { error } = await api.POST("/api/games/{id}/trade/offer", {
        params: { path: { id } },
        body: req,
      });
      set(
        error ? { status: "error", error: String(error) } : { status: "loaded" }
      );
    },

    acceptTrade: async (id, req) => {
      set({ status: "loading", error: null });
      const { error } = await api.POST("/api/games/{id}/trade/accept", {
        params: { path: { id } },
        body: req,
      });
      set(
        error ? { status: "error", error: String(error) } : { status: "loaded" }
      );
    },

    maritimeTrade4to1: async (id, req) => {
      set({ status: "loading", error: null });
      const { error } = await api.POST("/api/games/{id}/trade/maritime/4to1", {
        params: { path: { id } },
        body: req,
      });
      set(
        error ? { status: "error", error: String(error) } : { status: "loaded" }
      );
    },

    maritimeTrade3to1: async (id, req) => {
      set({ status: "loading", error: null });
      const { error } = await api.POST("/api/games/{id}/trade/maritime/3to1", {
        params: { path: { id } },
        body: req,
      });
      set(
        error ? { status: "error", error: String(error) } : { status: "loaded" }
      );
    },

    maritimeTrade2to1: async (id, req) => {
      set({ status: "loading", error: null });
      const { error } = await api.POST("/api/games/{id}/trade/maritime/2to1", {
        params: { path: { id } },
        body: req,
      });
      set(
        error ? { status: "error", error: String(error) } : { status: "loaded" }
      );
    },

    useRoadBuilding: async (id, req) => {
      set({ status: "loading", error: null });
      const { error } = await api.POST(
        "/api/games/{id}/devcards/road-building",
        {
          params: { path: { id } },
          body: req,
        }
      );
      set(
        error ? { status: "error", error: String(error) } : { status: "loaded" }
      );
    },

    useMonopoly: async (id, req) => {
      set({ status: "loading", error: null });
      const { data, error } = await api.POST(
        "/api/games/{id}/devcards/monopoly",
        {
          params: { path: { id } },
          body: req,
        }
      );
      if (error) {
        set({ status: "error", error: String(error) });
        return null;
      }
      set({ status: "loaded" });
      return data ?? null;
    },

    useYearOfPlenty: async (id, req) => {
      set({ status: "loading", error: null });
      const { data, error } = await api.POST(
        "/api/games/{id}/devcards/year-of-plenty",
        {
          params: { path: { id } },
          body: req,
        }
      );
      if (error) {
        set({ status: "error", error: String(error) });
        return null;
      }
      set({ status: "loaded" });
      return data ?? null;
    },

    useKnight: async (id, req) => {
      set({ status: "loading", error: null });
      const { error } = await api.POST("/api/games/{id}/devcards/knight", {
        params: { path: { id } },
        body: req,
      });
      set(
        error ? { status: "error", error: String(error) } : { status: "loaded" }
      );
    },

    endTurn: async (id) => {
      set({ status: "loading", error: null });
      const { data, error } = await api.POST("/api/games/{id}/turn/end", {
        params: { path: { id } },
      });
      if (error) {
        set({ status: "error", error: String(error) });
        return null;
      }
      set({ status: "loaded" });
      return data ?? null;
    },

    resolveRobber: async (id, req) => {
      set({ status: "loading", error: null });
      const { data, error } = await api.POST(
        "/api/games/{id}/turn/resolve-robber",
        {
          params: { path: { id } },
          body: req,
        }
      );
      if (error) {
        set({ status: "error", error: String(error) });
        return null;
      }
      set({ status: "loaded" });
      return data ?? null;
    },

    discardHalf: async (id, req) => {
      set({ status: "loading", error: null });
      const { error } = await api.POST("/api/games/{id}/turn/discard-half", {
        params: { path: { id } },
        body: req,
      });
      set(
        error ? { status: "error", error: String(error) } : { status: "loaded" }
      );
    },
  }))
);
