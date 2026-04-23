import { create } from "zustand";

/** One entry in the in-memory game chat log. */
export type GameChatEntry = {
  id: string;
  senderUserId: string;
  message: string;
  /** Server-issued UTC timestamp as epoch millis. */
  timestamp: number;
};

type State = {
  messages: GameChatEntry[];
};

type Actions = {
  append: (entry: Omit<GameChatEntry, "id">) => void;
  clear: () => void;
};

const MAX_MESSAGES = 200;

/**
 * Holds the rolling chat log for the currently-open game. Cleared on game
 * change in {@link GamePage}. Real-time appends come from the SignalR
 * `gameMessage` handler registered in {@link useGameSignalR}.
 */
export const useGameChatStore = create<State & Actions>((set) => ({
  messages: [],
  append: (entry) =>
    set((s) => ({
      messages: [
        ...s.messages,
        { ...entry, id: crypto.randomUUID() },
      ].slice(-MAX_MESSAGES),
    })),
  clear: () => set({ messages: [] }),
}));
