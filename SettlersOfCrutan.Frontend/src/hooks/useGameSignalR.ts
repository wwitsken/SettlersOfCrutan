import { useEffect } from "react";
import { useSignalRContext } from "../context/SignalRContext";
import { applyGamePayloadFromApi } from "../stores/game/applyPayload";
import { useGameStore } from "../stores/game";
import {
  RealtimeEvents,
  type GameChatMessagePayload,
  type GameEndedPayload,
} from "../api/realtimeEvents";

type GameReceiveArgs = [string, string | Date, string, unknown];

/**
 * Registers SignalR for the game route. Server contract:
 *  - Full game snapshots arrive as {@link RealtimeEvents.GameStateUpdated}
 *    with a per-user GameDto payload.
 *  - Chat messages arrive as {@link RealtimeEvents.GameMessage} with a
 *    {@link GameChatMessagePayload} payload and are appended to
 *    `useGameStore.chat`.
 *  - End-game overlay payloads arrive as {@link RealtimeEvents.GameEnded}
 *    and are stored in `useGameStore.gameOver`.
 */
export function useGameSignalR(gameId?: string | null) {
  const { isConnected, isConnecting, error, start, registerHandlers } =
    useSignalRContext();

  useEffect(() => {
    registerHandlers({
      GameReceive: (...args: unknown[]) => {
        const [evtGameId, timestamp, eventName, payload] =
          args as GameReceiveArgs;

        if (gameId && evtGameId && evtGameId !== gameId) return;

        if (eventName === RealtimeEvents.GameStateUpdated) {
          try {
            if (!applyGamePayloadFromApi(payload) && import.meta.env.DEV) {
              console.warn(
                "GameStateUpdated payload could not be mapped",
                payload,
              );
            }
          } catch (e) {
            console.error("Failed to project GameReceive payload:", e);
          }
          return;
        }

        if (eventName === RealtimeEvents.GameMessage) {
          const chat = payload as GameChatMessagePayload | null | undefined;
          if (!chat || typeof chat.message !== "string") {
            if (import.meta.env.DEV) {
              console.warn("gameMessage payload malformed", payload);
            }
            return;
          }
          const ts =
            timestamp instanceof Date
              ? timestamp.getTime()
              : typeof timestamp === "string"
                ? Date.parse(timestamp)
                : Date.now();
          useGameStore.getState().appendChat({
            senderUserId: chat.senderUserId,
            message: chat.message,
            timestamp: Number.isFinite(ts) ? ts : Date.now(),
          });
          return;
        }

        if (eventName === RealtimeEvents.GameEnded) {
          const ended = payload as GameEndedPayload | null | undefined;
          if (
            !ended ||
            typeof ended.winnerPlayerId !== "string" ||
            !Array.isArray(ended.finalScores)
          ) {
            if (import.meta.env.DEV) {
              console.warn("GameEnded payload malformed", payload);
            }
            return;
          }
          useGameStore.getState().setGameOver(ended);
          return;
        }

        if (import.meta.env.DEV) {
          console.debug(
            "[SignalR] GameReceive ignored (unknown event)",
            eventName,
          );
        }
      },
    });

    (async () => {
      try {
        await start();
      } catch {
        // connection errors are surfaced via context `error`
      }
    })();
  }, [gameId, registerHandlers, start]);

  return {
    isConnected,
    isConnecting,
    error,
  };
}
