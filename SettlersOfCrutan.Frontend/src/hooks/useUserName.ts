import { useState, useCallback } from "react";
import {
  fetchUserProfiles,
  type UserProfile,
} from "../api/userProfiles";

/**
 * Holds a set of resolved user profiles and exposes an imperative refresher.
 * Pass a pre-fetched list (typically from a route loader) as `initial` to
 * avoid flashing empty display names on first render.
 */
export function useUserProfiles(initial: readonly UserProfile[] = []) {
  const [users, setUsers] = useState<UserProfile[]>(() => [...initial]);

  const fetchProfiles = useCallback(async (userIds: readonly string[]) => {
    const fetched = await fetchUserProfiles(userIds);
    if (fetched.length === 0) return;
    setUsers((prev) => {
      const byId = new Map(prev.map((u) => [u.userId, u]));
      for (const u of fetched) byId.set(u.userId, u);
      return Array.from(byId.values());
    });
  }, []);

  return { users, fetchProfiles };
}
