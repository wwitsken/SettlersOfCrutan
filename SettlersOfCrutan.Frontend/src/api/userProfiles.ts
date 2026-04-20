import { api } from "./client";
import { getAccessTokenForOpenApi } from "../authConfig";
import type { components } from "./types";

export type UserProfile = components["schemas"]["UserProfileDto"];

/**
 * Fetch user profiles for the given set of user ids. Deduplicates input and
 * silently drops ids that fail to resolve (returning only the successful
 * profiles). Calls `/api/users/{userId}` per user because the bulk `/api/users`
 * endpoint expects a request body on GET, which browsers do not support from
 * `fetch`.
 */
export async function fetchUserProfiles(
  userIds: readonly string[],
): Promise<UserProfile[]> {
  const unique = Array.from(
    new Set(userIds.filter((id): id is string => typeof id === "string" && id.length > 0)),
  );
  if (unique.length === 0) return [];

  const accessToken = (await getAccessTokenForOpenApi()) ?? "";

  const results = await Promise.all(
    unique.map(async (userId) => {
      try {
        const { data, error } = await api.GET("/api/users/{userId}", {
          params: { path: { userId } },
          accessToken,
        });
        return error || !data ? null : data;
      } catch {
        return null;
      }
    }),
  );

  return results.filter((u): u is UserProfile => u !== null);
}
