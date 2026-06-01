import { useCallback, useEffect, useState } from "react";
import { useIsAuthenticated } from "@azure/msal-react";
import { api } from "../api/client";
import { readDevSessionUserIdFromStorage } from "../api/devSessionUser";
import type { components } from "../api/types";

type PlayerColor = components["schemas"]["PlayerColor"];

const PICKER_COLORS = ["red", "blue", "white", "orange", "green"] as const;

function normalizePickerColor(apiColor: string | undefined): string {
  if (!apiColor) return "blue";
  const lower = apiColor.toLowerCase();
  if ((PICKER_COLORS as readonly string[]).includes(lower)) return lower;
  return "blue";
}

/** Loads and persists display name + preferred color via `/api/me/profile` (creates `User` on first GET). */
export function useIdentity() {
  const msalAuthed = useIsAuthenticated();
  const [name, setNameState] = useState<string>(
    () => localStorage.getItem("catan_display_name") ?? "Adventurer",
  );
  const [color, setColorState] = useState<string>(
    () => localStorage.getItem("catan_color") ?? "blue",
  );
  const [profileLoading, setProfileLoading] = useState(false);
  const [profileError, setProfileError] = useState<string | null>(null);

  const shouldUseApi =
    msalAuthed ||
    (import.meta.env.DEV && readDevSessionUserIdFromStorage().length > 0);

  const refreshProfile = useCallback(async () => {
    if (!shouldUseApi) return;
    setProfileLoading(true);
    setProfileError(null);
    try {
      const { data, error, response } = await api.GET("/api/users/me", {});
      if (error || !data) {
        setProfileError(
          `Could not load profile (${response.status}). Using local settings.`,
        );
        return;
      }
      const nextName = data.displayName?.trim() || "Adventurer";
      const nextColor = normalizePickerColor(data.preferredColor);
      setNameState(nextName);
      setColorState(nextColor);
      localStorage.setItem("catan_display_name", nextName);
      localStorage.setItem("catan_color", nextColor);
    } finally {
      setProfileLoading(false);
    }
  }, [shouldUseApi]);

  useEffect(() => {
    void refreshProfile();
  }, [refreshProfile]);

  const setName = (n: string) => {
    const next = n;
    setNameState(next);
    localStorage.setItem("catan_display_name", next);
    if (!shouldUseApi) return;
    void (async () => {
      setProfileError(null);
      const { error, response } = await api.PUT("/api/users/me", {
        body: { displayName: next, preferredColor: null },
      });
      if (error) setProfileError(`Could not save name (${response.status}).`);
      else setProfileError(null);
    })();
  };

  const setColor = (c: string) => {
    setColorState(c);
    localStorage.setItem("catan_color", c);
    if (!shouldUseApi) return;
    void (async () => {
      setProfileError(null);
      const { error, response } = await api.PUT("/api/users/me", {
        body: { preferredColor: c as PlayerColor, displayName: null },
      });
      if (error) setProfileError(`Could not save color (${response.status}).`);
      else setProfileError(null);
    })();
  };

  return {
    name,
    color,
    setName,
    setColor,
    profileLoading,
    profileError,
    refreshProfile,
  };
}
