import { useMemo, useRef } from "react";
import { useUserProfilesStore } from "../stores/userProfiles";
import type { UserProfile } from "../api/userProfiles";

/**
 * Thin adapter over {@link useUserProfilesStore} kept to preserve the existing
 * `{ users, fetchProfiles }` call-site contract (see `LobbyPage`). `initial`
 * is merged into the shared store exactly once on mount so route-loader
 * payloads seed the cross-page cache.
 *
 * New code should prefer reading `useUserProfilesStore` directly.
 */
export function useUserProfiles(initial: readonly UserProfile[] = []) {
  const byId = useUserProfilesStore((s) => s.byId);
  const upsertProfiles = useUserProfilesStore((s) => s.upsertProfiles);
  const ensureProfilesFor = useUserProfilesStore((s) => s.ensureProfilesFor);

  const seededRef = useRef(false);
  if (!seededRef.current && initial.length > 0) {
    seededRef.current = true;
    upsertProfiles(initial);
  }

  const users = useMemo(() => Object.values(byId), [byId]);
  return { users, fetchProfiles: ensureProfilesFor };
}
