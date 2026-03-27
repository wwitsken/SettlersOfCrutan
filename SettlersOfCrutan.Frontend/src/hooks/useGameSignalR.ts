import { useEffect } from "react";
import { useSignalRContext } from "../context/SignalRContext";
import { useGamesStore } from "../stores/gameStore";
import { gamePayloadToDomain } from "../domain/game/mapGameFromApi";
import { RealtimeEvents } from "../realtime/realtimeEvents";

type GameReceiveArgs = [string, string | Date, string, unknown];

/**
 * Registers SignalR for the game route. Server contract: full snapshots use
 * {@link RealtimeEvents.GameStateUpdated} with a per-user GameDto payload.
 */
export function useGameSignalR(gameId?: string | null) {
  const { isConnected, isConnecting, error, start, registerHandlers } =
    useSignalRContext();

  const setGame = useGamesStore((s) => s.setGame);

  useEffect(() => {
    registerHandlers({
      GameReceive: (...args: unknown[]) => {
        const [evtGameId, , eventName, payload] = args as GameReceiveArgs;

        if (gameId && evtGameId && evtGameId !== gameId) return;

        if (eventName !== RealtimeEvents.GameStateUpdated) {
          if (import.meta.env.DEV) {
            console.debug(
              "[SignalR] GameReceive ignored (unknown event)",
              eventName,
            );
          }
          return;
        }

        try {
          const projected = gamePayloadToDomain(payload);
          if (projected) setGame(projected);
          else if (import.meta.env.DEV) {
            console.warn("GameStateUpdated payload could not be mapped", payload);
          }
        } catch (e) {
          console.error("Failed to project GameReceive payload:", e);
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
  }, [gameId, registerHandlers, setGame, start]);

  return {
    isConnected,
    isConnecting,
    error,
  };
}
