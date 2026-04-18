import { create } from "zustand";

export type GameToast = {
  id: string;
  message: string;
};

type State = {
  toasts: GameToast[];
};

type Actions = {
  push: (message: string) => void;
  dismiss: (id: string) => void;
  clear: () => void;
};

const MAX_TOASTS = 5;

export const useGameToastStore = create<State & Actions>((set) => ({
  toasts: [],
  push: (message) =>
    set((s) => ({
      toasts: [
        ...s.toasts,
        { id: crypto.randomUUID(), message },
      ].slice(-MAX_TOASTS),
    })),
  dismiss: (id) =>
    set((s) => ({
      toasts: s.toasts.filter((t) => t.id !== id),
    })),
  clear: () => set({ toasts: [] }),
}));
