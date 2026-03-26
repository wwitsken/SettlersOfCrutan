import { useEffect } from "react";
import { useSignalRContext } from "../context/SignalRContext";
import { useGamesStore } from "../stores/gameStore";
import type { Game } from "../domain/game/game";

type GameReceiveArgs = [string, Date, string, unknown];

/**
 * Registers SignalR handlers for the Game page.
 *
 * This mirrors the LobbyPage pattern, but keeps handler registration in a hook.
 * The payload projection is intentionally lightweight/mocked for now.
 */
export function useGameSignalR(gameId?: string | null) {
  const { isConnected, isConnecting, error, start, registerHandlers } =
    useSignalRContext();

  const setGame = useGamesStore((s) => s.setGame);

  useEffect(() => {
    registerHandlers({
      GameReceive: (...args: unknown[]) => {
        const [evtGameId, timestamp, eventName, payload] =
          args as GameReceiveArgs;

        // If this page is scoped to a specific game, ignore events for other games.
        if (gameId && evtGameId && evtGameId !== gameId) return;

        try {
          // TODO: replace with proper projection from API/SignalR DTO -> domain Game
          const projected = (payload ?? {}) as Game;
          setGame(projected);

          console.log(
            `gameId: ${evtGameId}\n timeStamp: ${timestamp}\n eventName: ${eventName}\n payLoad: ${JSON.stringify(
              projected,
            )}`,
          );
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
