import { useMemo } from "react";
import { useGameStore } from "../stores/game";
import { selectMeAndTurn, type MeAndTurn } from "../domain/game/selectors";
import type { Game } from "../domain/game/game";

/**
 * Resolves `{ me, currentPlayerId, isMyTurn }` against the given game snapshot
 * (typically the enriched one from {@link useEnrichedGame}, so that
 * `me.displayName` reflects the fetched user profile) and the private slice
 * from {@link useGameStore}.
 */
export function useMeAndTurn(enrichedGame: Game | null): MeAndTurn {
  const privateGame = useGameStore((s) => s.privateGame);
  return useMemo(
    () => selectMeAndTurn(enrichedGame, privateGame),
    [enrichedGame, privateGame],
  );
}
