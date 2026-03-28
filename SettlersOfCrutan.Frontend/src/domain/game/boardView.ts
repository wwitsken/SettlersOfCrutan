import type { Game } from "./game";

export type BoardView = "loading" | "live" | "example";

export type GameStoreStatus = "idle" | "loading" | "loaded" | "error";

/**
 * What the 3D board is showing vs what is in Zustand `game`.
 */
export function resolveBoardView(
  status: GameStoreStatus,
  game: Game | null,
): BoardView {
  if (status === "loading") return "loading";
  if (game) return "live";
  return "example";
}
