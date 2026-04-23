import { useEffect } from "react";
import { useGameStore } from "../stores/game";
import { applyGamePayloadFromApi } from "../stores/game/applyPayload";
import { useGameSignalR } from "./useGameSignalR";
import type { components } from "../api/types";
import type { UserProfile } from "../api/userProfiles";

type GameDto = components["schemas"]["GameDto"];

export type GameLoaderResult = {
  loadedStatus: number;
  gameData: GameDto | null;
  users: UserProfile[];
};

/**
 * Owns the per-`gameId` page lifecycle: resets per-game store slices, applies
 * the loader's initial payload (or records the loader error), and subscribes
 * to SignalR updates. Returns the SignalR connection status so callers can
 * disable chat controls while offline.
 */
export function useGameLifecycle(
  gameId: string | null,
  loader: GameLoaderResult,
) {
  const resetForGame = useGameStore((s) => s.resetForGame);
  const setError = useGameStore((s) => s.setError);

  useEffect(() => {
    if (!gameId) return;
    resetForGame(gameId);

    if (loader.loadedStatus !== 200 || !loader.gameData) {
      setError(`Game request failed (${loader.loadedStatus}).`);
      return;
    }

    if (!applyGamePayloadFromApi(loader.gameData)) {
      setError("Game response could not be mapped.");
    }
  }, [gameId, loader, resetForGame, setError]);

  return useGameSignalR(gameId);
}
