import type { UserProfile } from "../../api/userProfiles";
import type { Game } from "./game";

/** Overlay profile display names onto the public game roster (store + toasts). */
export function enrichGamePlayerDisplayNames(
  game: Game,
  byId: Record<string, UserProfile>,
): Game {
  return {
    ...game,
    players: game.players.map((p) => {
      const fromProfile = byId[p.userId]?.displayName?.trim();
      const fromPlayer = p.displayName?.trim();
      return {
        ...p,
        displayName: fromProfile || fromPlayer || "",
      };
    }),
  };
}
