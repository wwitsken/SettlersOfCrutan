import type { PlayerColor } from "./gameTypes";

/** Hex colors for board pieces and UI chips (matches `GamePlayersTurnBar`). */
export const PLAYER_COLOR_HEX: Record<PlayerColor, string> = {
  none: "#94a3b8",
  red: "#dc2626",
  blue: "#2563eb",
  white: "#f8fafc",
  orange: "#ea580c",
  green: "#16a34a",
  yellow: "#ca8a04",
  brown: "#78350f",
  purple: "#7c3aed",
};

export function playerColorToHex(color: PlayerColor | undefined): string {
  if (!color || color === "none") return PLAYER_COLOR_HEX.none;
  return PLAYER_COLOR_HEX[color] ?? PLAYER_COLOR_HEX.none;
}
