import { create } from "zustand";
import { immer } from "zustand/middleware/immer";
import { fetchUserProfiles, type UserProfile } from "../../api/userProfiles";

type State = {
  byId: Record<string, UserProfile>;
  /** Sorted-id cache key of the last set of ids resolved via `ensureProfilesFor`. */
  lastEnsuredKey: string | null;
};

type Actions = {
  upsertProfiles: (profiles: readonly UserProfile[]) => void;
  /**
   * Fetch and merge profiles for `userIds`, dedup'd by a sorted-id cache key.
   * Concurrent callers with the same roster are coalesced into a single fetch.
   */
  ensureProfilesFor: (userIds: readonly string[]) => Promise<void>;
};

/**
 * Cross-page user-profile cache. Seeded synchronously by route loaders and
 * refreshed on roster changes by `useEnrichedGame`. Lives outside the per-game
 * `useGameStore` so a lobby -> game navigation reuses already-fetched profiles.
 */
export const useUserProfilesStore = create<State & Actions>()(
  immer((set, get) => ({
    byId: {},
    lastEnsuredKey: null,

    upsertProfiles: (profiles) =>
      set((s) => {
        for (const p of profiles) {
          if (typeof p.userId === "string" && p.userId.length > 0) {
            s.byId[p.userId] = p;
          }
        }
      }),

    ensureProfilesFor: async (userIds) => {
      const clean = Array.from(
        new Set(
          userIds.filter(
            (id): id is string => typeof id === "string" && id.length > 0,
          ),
        ),
      );
      if (clean.length === 0) return;

      const key = [...clean].sort().join(",");
      if (key === get().lastEnsuredKey) return;

      set((s) => {
        s.lastEnsuredKey = key;
      });

      const fetched = await fetchUserProfiles(clean);
      if (fetched.length === 0) return;

      set((s) => {
        for (const p of fetched) {
          if (typeof p.userId === "string" && p.userId.length > 0) {
            s.byId[p.userId] = p;
          }
        }
      });
    },
  })),
);

export type { UserProfile };
