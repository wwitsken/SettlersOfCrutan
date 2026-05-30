import { useEffect, useMemo } from "react";
import { enrichGamePlayerDisplayNames } from "../domain/game/enrichGamePlayers";
import type { Game } from "../domain/game/game";
import { useGameStore } from "../stores/game";
import { useUserProfilesStore } from "../stores/userProfiles";

/**
 * Returns the current game with every player's `displayName` overlaid with the
 * latest value from {@link useUserProfilesStore}. Also ensures the profile
 * store has been asked to fetch entries for every player in the roster; the
 * store internally dedupes by the roster's sorted-id key so roster-change
 * payloads from SignalR only trigger a fetch when the set actually changes.
 *
 * Profiles are expected to be seeded by the route loader (`GameLoader`) so the
 * first paint already reflects display names without flashing.
 */
export function useEnrichedGame(): Game | null {
  const game = useGameStore((s) => s.game);
  const byId = useUserProfilesStore((s) => s.byId);
  const ensureProfilesFor = useUserProfilesStore((s) => s.ensureProfilesFor);

  useEffect(() => {
    if (!game?.players.length) return;
    const ids = game.players
      .map((p) => p.userId)
      .filter((id): id is string => typeof id === "string" && id.length > 0);
    if (ids.length) void ensureProfilesFor(ids);
  }, [game, ensureProfilesFor]);

  return useMemo(() => {
    if (!game) return null;
    return enrichGamePlayerDisplayNames(game, byId);
  }, [game, byId]);
}
