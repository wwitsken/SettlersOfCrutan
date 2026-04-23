import { useCallback, useMemo } from "react";
import { useSignalRContext } from "../context/SignalRContext";
import { useGameStore } from "../stores/game";
import { useUserProfilesStore } from "../stores/userProfiles";
import type { ChatMessage } from "../domain/game/gameTypes";

/**
 * Returns the chat message list for the current game, projected into the
 * display-ready {@link ChatMessage} shape (sender display name + color
 * resolved from the loaded game roster), plus a `sendMessage` helper that
 * invokes the `SendGameMessage` hub method over SignalR.
 *
 * Messages are populated by `useGameSignalR`; sender display names are
 * enriched from the shared {@link useUserProfilesStore}, so callers no longer
 * need to pass roster overrides.
 */
export function useGameChat(gameId: string | null | undefined) {
  const { invoke } = useSignalRContext();
  const entries = useGameStore((s) => s.chat);
  const players = useGameStore((s) => s.game?.players);
  const byId = useUserProfilesStore((s) => s.byId);

  const messages = useMemo<ChatMessage[]>(() => {
    const playersByUser = new Map((players ?? []).map((p) => [p.userId, p]));
    return entries.map((e) => {
      const player = playersByUser.get(e.senderUserId);
      const displayName =
        byId[e.senderUserId]?.displayName || player?.displayName || "";
      return {
        id: e.id,
        player: displayName || "Unknown",
        color: player?.playerColor ?? "none",
        text: e.message,
        time: new Date(e.timestamp).toLocaleTimeString([], {
          hour: "2-digit",
          minute: "2-digit",
        }),
      } satisfies ChatMessage;
    });
  }, [entries, players, byId]);

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

  return { messages, sendMessage };
}
