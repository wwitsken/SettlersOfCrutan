import type { Game } from "./game";
import type { HexCoordinate } from "./board";
import { hexEq } from "./hexCoords";

/** Players with a building touching this hex (backend IsPlayerExposedToHex). */
export function getPlayerIdsExposedToHex(
  game: Game,
  hex: HexCoordinate,
): string[] {
  const ids = new Set<string>();
  for (const pc of game.board.populationCenters) {
    if (pc.coordinates.some((c) => hexEq(c, hex))) {
      ids.add(pc.playerOwnerId);
    }
  }
  return [...ids];
}
