// src/auth/store.ts
import { create } from "zustand";
import {
  subscribeWithSelector,
  persist,
  createJSONStorage,
} from "zustand/middleware";
import { api } from "../api/client";

export type User = {
  userId: string;
  email: string;
  roles: string[];
};

type Status = "idle" | "loading" | "unauthenticated" | "authenticated";

type LoginBody = {
  email: string;
  password: string;
};

type AuthState = {
  status: Status;
  user: User | null;

  // derived helpers (as functions so they always read latest state)
  isAuthed: () => boolean;
  hasRole: (role: string) => boolean;
  hasAnyRole: (roles: string[]) => boolean;

  // actions
  init: () => Promise<void>; // call once on app start or tab focus
  refresh: () => Promise<void>;
  login: (body: LoginBody) => Promise<void>;
  logout: () => Promise<void>;
};

export const useAuthStore = create<AuthState>()(
  subscribeWithSelector(
    persist(
      (set, get) => ({
        status: "idle",
        user: null,

        isAuthed: () => get().status === "authenticated",
        hasRole: (role: string) =>
          get().status === "authenticated" &&
          !!get().user?.roles.includes(role),
        hasAnyRole: (roles: string[]) =>
          get().status === "authenticated" &&
          roles.some((r) => get().user?.roles.includes(r)),

        init: async () => {
          // Avoid noisy re-fetch on every cold start: do a quick optimistic check
          if (get().status === "idle") set({ status: "loading" });

          const { data, error } = await api.GET("/api/auth/me");
          if (error) {
            set({ status: "unauthenticated", user: null });
            throw new Error(error);
          }
          set(
            data
              ? { status: "authenticated", user: { ...data } }
              : { status: "unauthenticated", user: null }
          );
        },

        refresh: async () => {
          set({ status: "loading" });
          const { data, error } = await api.GET("/api/auth/me");
          if (error) {
            set({ status: "unauthenticated", user: null });
            throw new Error(error);
          }
          set(
            data
              ? { status: "authenticated", user: { ...data } }
              : { status: "unauthenticated", user: null }
          );
        },

        login: async (body) => {
          set({ status: "loading" });
          const { error } = await api.POST("/api/auth/login", { body });
          if (error) {
            set({ status: "unauthenticated", user: null });
            throw new Error(error);
          }
          const meRes = await api.GET("/api/auth/me");
          if (meRes.error) {
            set({ status: "unauthenticated", user: null });
            throw new Error(error);
          }
          set(
            meRes.data
              ? { status: "authenticated", user: { ...meRes.data } }
              : { status: "unauthenticated", user: null }
          );
        },

        logout: async () => {
          await api.POST("/api/auth/logout");
          set({ status: "unauthenticated", user: null });
        },
      }),
      {
        name: "auth", // persisted key
        storage: createJSONStorage(() => sessionStorage),
        partialize: (s) => ({ user: s.user, status: s.status }),
      }
    )
  )
);
