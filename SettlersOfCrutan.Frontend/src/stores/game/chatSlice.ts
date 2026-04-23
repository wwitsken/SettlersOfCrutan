import type { GameChatEntry, GameSliceCreator } from "./types";

const MAX_MESSAGES = 200;

type ChatSlice = {
  chat: GameChatEntry[];
  appendChat: (entry: Omit<GameChatEntry, "id">) => void;
};

/**
 * Rolling chat log for the currently-open game. Cleared on game change via
 * `resetForGame`. Real-time appends come from the SignalR `gameMessage`
 * handler registered in `useGameSignalR`.
 */
export const createChatSlice: GameSliceCreator<ChatSlice> = (set) => ({
  chat: [],
  appendChat: (entry) =>
    set((s) => {
      s.chat.push({ ...entry, id: crypto.randomUUID() });
      if (s.chat.length > MAX_MESSAGES) {
        s.chat.splice(0, s.chat.length - MAX_MESSAGES);
      }
    }),
});
