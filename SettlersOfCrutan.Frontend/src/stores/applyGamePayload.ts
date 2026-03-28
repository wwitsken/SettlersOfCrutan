import { mapGamePayload } from "../domain/game/mapGameFromApi";
import { useGamesStore } from "./gameStore";

/**
 * Map a raw HTTP/SignalR payload and write public + private slices. Returns whether mapping succeeded.
 */
export function applyGamePayloadFromApi(payload: unknown): boolean {
  const mapped = mapGamePayload(payload);
  if (!mapped) return false;
  useGamesStore.getState().applyLoadedState(mapped.game, mapped.privateGame);
  return true;
}
