import type { Game } from "./game";
import type { HexCoordinate, PopulationCenter, Port } from "./board";
import type { ResourceCardType } from "./gameTypes";

/** Matches server maritime endpoints: 4:1, 3:1 (generic port), 2:1 (resource-specific port). */
export type MaritimeRatio = 4 | 3 | 2;

function hexKey(h: HexCoordinate): string {
  const s = h.s ?? -h.q - h.r;
  return `${h.q},${h.r},${s}`;
}

/**
 * Settlement sits on this port’s vertex iff the vertex’s three hexes include
 * both hexes of the port edge (matches domain PortVertexAdjacency).
 */
export function portTouchesPopulationCenter(
  port: Port,
  pc: PopulationCenter,
): boolean {
  const edgeHexes = [port.inCoordinate, port.outCoordinate];
  const vertexKeys = new Set(pc.coordinates.map(hexKey));
  return edgeHexes.every((h) => vertexKeys.has(hexKey(h)));
}

function myPopulationCenters(
  game: Game,
  myPlayerId: string,
): PopulationCenter[] {
  return game.board.populationCenters.filter(
    (pc) => pc.playerOwnerId === myPlayerId,
  );
}

export function playerTouchesPortType(
  game: Game,
  myPlayerId: string,
  portType: string,
): boolean {
  const ports = game.board.ports.filter((p) => p.type === portType);
  if (ports.length === 0) return false;
  const pcs = myPopulationCenters(game, myPlayerId);
  return ports.some((port) =>
    pcs.some((pc) => portTouchesPopulationCenter(port, pc)),
  );
}

const RESOURCE_TO_2TO1_PORT: Partial<Record<ResourceCardType, string>> = {
  brick: "brick2to1",
  lumber: "lumber2to1",
  wool: "wool2to1",
  grain: "grain2to1",
  ore: "ore2to1",
};

/**
 * Best single-trade ratio for giving `discard` away, mirroring domain priority:
 * 2:1 resource port if eligible, else 3:1 generic port, else 4:1 (no port required).
 */
export function bestMaritimeRatio(
  game: Game,
  myPlayerId: string,
  discard: ResourceCardType,
  handCount: number,
): MaritimeRatio | null {
  const port2 = RESOURCE_TO_2TO1_PORT[discard];
  if (
    handCount >= 2 &&
    port2 &&
    playerTouchesPortType(game, myPlayerId, port2)
  ) {
    return 2;
  }
  if (handCount >= 3 && playerTouchesPortType(game, myPlayerId, "generic3to1")) {
    return 3;
  }
  if (handCount >= 4) return 4;
  return null;
}
