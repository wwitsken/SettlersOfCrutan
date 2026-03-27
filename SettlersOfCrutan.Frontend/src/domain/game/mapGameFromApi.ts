import type { Game } from "./game";
import type { Board, Hex, HexCoordinate, PopulationCenter, Port, Road } from "./board";
import type { Player } from "./player";
import type { TradeOffer } from "./tradeOffer";
import type { components } from "../../api/types";

type PublicGameDto = components["schemas"]["PublicGameDto"];
type HexDto = components["schemas"]["HexDto"];
type HexCoordinateDto = components["schemas"]["HexCoordinateDto"];

function num(v: number | string | undefined, fallback = 0): number {
  if (v === undefined || v === null) return fallback;
  return typeof v === "number" ? v : Number(v);
}

function toHexCoord(dto: HexCoordinateDto | undefined): HexCoordinate {
  const q = num(dto?.q);
  const r = num(dto?.r);
  const s =
    dto && "s" in dto && dto.s !== undefined && dto.s !== null
      ? num(dto.s as number | string)
      : -q - r;
  return { q, r, s };
}

function mapHex(dto: HexDto | undefined): Hex | null {
  if (!dto?.coordinate) return null;
  return {
    coordinate: toHexCoord(dto.coordinate),
    resource: dto.resource ?? "desert",
    numberToken: num(dto.numberToken),
    hasRobber: !!dto.hasRobber,
  };
}

function mapBoard(dto: PublicGameDto["board"] | undefined): Board {
  if (!dto) {
    return { hexes: [], populationCenters: [], roads: [], ports: [] };
  }
  const hexes = (dto.hexes ?? [])
    .map(mapHex)
    .filter((h): h is Hex => h !== null);
  const populationCenters: PopulationCenter[] = (dto.populationCenters ?? [])
    .filter((p) => p.coordinates?.length)
    .map((p) => ({
      coordinates: (p.coordinates ?? []).map(toHexCoord),
      type: p.type === "city" ? "city" : "settlement",
      playerOwnerId: p.playerOwnerId ?? "",
    }));
  const roads: Road[] = (dto.roads ?? [])
    .filter((r) => r.coordinates?.length)
    .map((r) => ({
      coordinates: (r.coordinates ?? []).map(toHexCoord),
      playerOwnerId: r.playerOwnerId ?? "",
    }));
  const ports: Port[] = (dto.ports ?? [])
    .filter((p) => p.inCoordinate && p.outCoordinate)
    .map((p) => ({
      inCoordinate: toHexCoord(p.inCoordinate),
      outCoordinate: toHexCoord(p.outCoordinate),
      type: p.type ?? "",
    }));
  return { hexes, populationCenters, roads, ports };
}

function mapPlayers(list: PublicGameDto["players"]): Player[] {
  return (list ?? []).map((p) => ({
    id: p.id,
    playOrder: num(p.playOrder),
    isPlaying: !!p.isPlaying,
    displayName: p.displayName ?? "",
    playerColor: p.playerColor ?? "none",
    resourceCardCount: num(p.resourceCardCount),
    developmentCardCount: num(p.developmentCardCount),
    pieceReserve: Object.fromEntries(
      Object.entries(p.pieceReserve ?? {}).map(([k, v]) => [k, num(v)]),
    ),
    discardRequirement: num(p.discardRequirement),
  }));
}

function mapTradeOffer(
  dto: PublicGameDto["currentTradeOffer"],
): TradeOffer | undefined {
  if (!dto) return undefined;
  return {
    playerProposerId: dto.playerProposerId ?? "",
    playerAcceptorId: dto.playerAcceptorId ?? undefined,
    requestedResources: Object.fromEntries(
      Object.entries(dto.requestedResources ?? {}).map(([k, v]) => [k, num(v)]),
    ),
    offeredResources: Object.fromEntries(
      Object.entries(dto.offeredResources ?? {}).map(([k, v]) => [k, num(v)]),
    ),
    isAccepted: !!dto.isAccepted,
  };
}

/**
 * Normalize SignalR GameStateUpdated payload: either wrapped GameDto `{ game, myPrivateGameInfo }`
 * or a flat public game DTO.
 */
export function gamePayloadToDomain(payload: unknown): Game | null {
  if (!payload || typeof payload !== "object") return null;
  const root = payload as Record<string, unknown>;
  const publicPart =
    root.game && typeof root.game === "object"
      ? (root.game as PublicGameDto)
      : (root as unknown as PublicGameDto);

  const id = publicPart.id;
  if (!id) return null;

  const players = mapPlayers(publicPart.players);
  const currentId =
    publicPart.currentPlayerId != null
      ? String(publicPart.currentPlayerId as object & string)
      : undefined;
  const playerIndex = currentId
    ? Math.max(
        0,
        players.findIndex((p) => p.id === currentId || p.id === String(currentId)),
      )
    : 0;

  return {
    id,
    gameType: publicPart.gameType ?? "baseGame",
    gameName: publicPart.gameName,
    board: mapBoard(publicPart.board),
    bankResourceHand: Object.fromEntries(
      Object.entries(publicPart.bankResourceHand ?? {}).map(([k, v]) => [
        k,
        num(v),
      ]),
    ),
    bankDevCardHand: Object.fromEntries(
      Object.entries(publicPart.bankDevCardHand ?? {}).map(([k, v]) => [
        k,
        num(v),
      ]),
    ),
    turnExpiresAt: publicPart.turnExpiresAt
      ? new Date(publicPart.turnExpiresAt)
      : undefined,
    playerDirection: publicPart.playerDirection ?? "clockwise",
    gamePhase: publicPart.gamePhase ?? "pendingStart",
    round: num(publicPart.round),
    playerIndex: Number.isFinite(playerIndex) && playerIndex >= 0 ? playerIndex : 0,
    currentTradeOffer: mapTradeOffer(publicPart.currentTradeOffer),
    players,
  };
}
