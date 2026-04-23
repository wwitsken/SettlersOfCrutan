import { useCallback, useMemo } from "react";
import { useSignalRContext } from "../context/SignalRContext";
import { useGameChatStore } from "../stores/gameChatStore";
import { useGamesStore } from "../stores/gameStore";
import type { ChatMessage } from "../domain/game/gameTypes";
import type { Player } from "../domain/game/player";

/**
 * Returns the chat message list for the current game, projected into the
 * display-ready {@link ChatMessage} shape (sender display name + color
 * resolved from the loaded game roster), plus a `sendMessage` helper that
 * invokes the `SendGameMessage` hub method over SignalR.
 *
 * Messages are populated by {@link useGameSignalR}.
 *
 * `playersOverride` lets callers pass roster entries whose `displayName` has
 * already been enriched from the user-profile lookup (`GamePage.enrichedGame`).
 * The raw store roster carries empty display names, so without the override
 * every sender renders as "Unknown".
 */
export function useGameChat(
  gameId: string | null | undefined,
  playersOverride?: readonly Player[],
) {
  const { invoke, isConnected } = useSignalRContext();
  const entries = useGameChatStore((s) => s.messages);
  const storePlayers = useGamesStore((s) => s.game?.players);
  const players = playersOverride ?? storePlayers;

  const messages = useMemo<ChatMessage[]>(() => {
    const byUserId = new Map(
      (players ?? []).map((p) => [
        p.userId,
        { displayName: p.displayName, color: p.playerColor },
      ]),
    );
    return entries.map((e) => {
      const who = byUserId.get(e.senderUserId);
      return {
        id: e.id,
        player: who?.displayName || "Unknown",
        color: who?.color ?? "none",
        text: e.message,
        time: new Date(e.timestamp).toLocaleTimeString([], {
          hour: "2-digit",
          minute: "2-digit",
        }),
      } satisfies ChatMessage;
    });
  }, [entries, players]);

  const sendMessage = useCallback(
    async (text: string) => {
      const trimmed = text.trim();
      if (!trimmed || !gameId) return;
      try {
        await invoke("SendGameMessage", gameId, trimmed);
      } catch (e) {
        console.error("Failed to send chat message:", e);
      }
    },
    [invoke, gameId],
  );

  return { messages, sendMessage, isConnected };
}
