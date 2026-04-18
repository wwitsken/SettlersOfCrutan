import { snapshotToastMessages } from "../domain/game/snapshotToastMessages";
import { mapGamePayload } from "../domain/game/mapGameFromApi";
import { useGamesStore } from "./gameStore";
import { useGameToastStore } from "./gameToastStore";

/**
 * Map a raw HTTP/SignalR payload and write public + private slices. Returns whether mapping succeeded.
 */
export function applyGamePayloadFromApi(payload: unknown): boolean {
  const prevGame = useGamesStore.getState().game;

  const mapped = mapGamePayload(payload);
  if (!mapped) return false;

  useGamesStore.getState().applyLoadedState(mapped.game, mapped.privateGame);

  const lines = snapshotToastMessages(
    prevGame,
    mapped.game,
    mapped.privateGame,
  );
  const toast = useGameToastStore.getState();
  for (const message of lines) {
    toast.push(message);
  }

  return true;
}
