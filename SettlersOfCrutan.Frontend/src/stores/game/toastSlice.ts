import type { GameSliceCreator, GameToast } from "./types";

export const MAX_TOASTS = 5;

type ToastSlice = {
  toasts: GameToast[];
  pushToast: (message: string) => void;
  dismissToast: (id: string) => void;
};

export const createToastSlice: GameSliceCreator<ToastSlice> = (set) => ({
  toasts: [],
  pushToast: (message) =>
    set((s) => {
      s.toasts.push({ id: crypto.randomUUID(), message });
      if (s.toasts.length > MAX_TOASTS) {
        s.toasts.splice(0, s.toasts.length - MAX_TOASTS);
      }
    }),
  dismissToast: (id) =>
    set((s) => {
      s.toasts = s.toasts.filter((t) => t.id !== id);
    }),
});
