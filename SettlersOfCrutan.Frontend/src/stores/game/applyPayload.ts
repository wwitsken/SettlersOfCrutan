import { enrichGamePlayerDisplayNames } from "../../domain/game/enrichGamePlayers";
import { mapGamePayload } from "../../domain/game/mapGameFromApi";
import { snapshotToastMessages } from "../../domain/game/snapshotToastMessages";
import { useUserProfilesStore } from "../userProfiles";
import { MAX_TOASTS } from "./toastSlice";
import { useGameStore } from "./index";

/**
 * Map a raw HTTP/SignalR payload and write the public + private game slices
 * alongside any derived toast lines in a single transaction. Returns whether
 * mapping succeeded.
 */
export function applyGamePayloadFromApi(payload: unknown): boolean {
  const mapped = mapGamePayload(payload);
  if (!mapped) return false;

  useGameStore.setState((s) => {
    const prev = s.game;
    s.game = mapped.game;
    s.currentGameId = mapped.game.id;
    s.status = "loaded";
    s.error = null;
    s.privateGame = mapped.privateGame;

    const nextForToasts = enrichGamePlayerDisplayNames(
      mapped.game,
      useUserProfilesStore.getState().byId,
    );
    const lines = snapshotToastMessages(prev, nextForToasts, mapped.privateGame);
    for (const message of lines) {
      s.toasts.push({ id: crypto.randomUUID(), message });
    }
    if (s.toasts.length > MAX_TOASTS) {
      s.toasts.splice(0, s.toasts.length - MAX_TOASTS);
    }
  });

  return true;
}
