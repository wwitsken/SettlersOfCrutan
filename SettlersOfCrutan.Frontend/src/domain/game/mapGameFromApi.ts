import type { Game } from "./game";
import type { Board, Hex, HexCoordinate, PopulationCenter, Port, Road } from "./board";
import type { Player } from "./player";
import type { PrivateGameInfo, MyHand } from "./privateGame";
import type { TradeOffer } from "./tradeOffer";
import type {
  GamePhase,
  GameType,
  PlayerColor,
  PlayerDirection,
  ResourceCardType,
} from "./gameTypes";
import type { components } from "../../api/types";

type PublicGameDto = components["schemas"]["PublicGameDto"];
type HexDto = components["schemas"]["HexDto"];
type HexCoordinateDto = components["schemas"]["HexCoordinateDto"];

const RESOURCE_VALUES: ResourceCardType[] = [
  "none",
  "brick",
  "lumber",
  "wool",
  "grain",
  "ore",
  "desert",
  "water",
];

const PLAYER_COLOR_VALUES: PlayerColor[] = [
  "none",
  "red",
  "blue",
  "white",
  "orange",
  "green",
  "yellow",
  "brown",
  "purple",
];

const GAME_PHASE_VALUES: GamePhase[] = [
  "pendingStart",
  "setup",
  "rollDice",
  "discardHalf",
  "resolveRobber",
  "tradeBuild",
  "gameEnd",
];

const PLAYER_DIRECTION_VALUES: PlayerDirection[] = [
  "clockwise",
  "counterClockwise",
];

const GAME_TYPE_VALUES: GameType[] = [
  "baseGame",
  "seafarers",
  "citiesAndKnights",
  "tradersAndBarbarians",
  "explorersAndPirates",
];

function isRecord(v: unknown): v is Record<string, unknown> {
  return v !== null && typeof v === "object" && !Array.isArray(v);
}

function hasWrappedPublicGame(root: Record<string, unknown>): boolean {
  const g = root.game;
  return g !== null && typeof g === "object" && !Array.isArray(g);
}

function hasPublicGameShape(o: unknown): o is PublicGameDto {
  if (!isRecord(o)) return false;
  if (!("id" in o) || o.id == null) return false;
  const b = o.board;
  if (b === null || typeof b !== "object" || Array.isArray(b)) return false;
  return true;
}

/**
 * Extract `PublicGameDto` from either a wrapped `{ game }` or a flat public body.
 */
export function extractPublicGameDto(payload: unknown): PublicGameDto | null {
  if (!isRecord(payload)) return null;
  if (hasWrappedPublicGame(payload)) {
    const inner = payload.game;
    return hasPublicGameShape(inner) ? inner : null;
  }
  return hasPublicGameShape(payload) ? payload : null;
}

const RESOURCE_ALIASES: Record<string, ResourceCardType> = {
  wood: "lumber",
  sheep: "wool",
  wheat: "grain",
};

function normalizeResourceCardType(v: string | undefined): ResourceCardType {
  if (!v) return "desert";
  if ((RESOURCE_VALUES as readonly string[]).includes(v))
    return v as ResourceCardType;
  const aliased = RESOURCE_ALIASES[v.toLowerCase()];
  if (aliased) return aliased;
  return "desert";
}

function normalizePlayerColor(v: string | undefined): PlayerColor {
  if (v && (PLAYER_COLOR_VALUES as readonly string[]).includes(v))
    return v as PlayerColor;
  return "none";
}

function normalizeGamePhase(v: string | undefined): GamePhase {
  if (v && (GAME_PHASE_VALUES as readonly string[]).includes(v))
    return v as GamePhase;
  return "pendingStart";
}

function normalizePlayerDirection(v: string | undefined): PlayerDirection {
  if (v && (PLAYER_DIRECTION_VALUES as readonly string[]).includes(v))
    return v as PlayerDirection;
  return "clockwise";
}

function normalizeGameType(v: string | undefined): GameType {
  if (v && (GAME_TYPE_VALUES as readonly string[]).includes(v))
    return v as GameType;
  return "baseGame";
}

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
    resource: normalizeResourceCardType(dto.resource),
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
    playerColor: normalizePlayerColor(p.playerColor),
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

function mapStringNumDict(v: unknown): Record<string, number> {
  if (!v || typeof v !== "object") return {};
  return Object.fromEntries(
    Object.entries(v as Record<string, unknown>).map(([k, val]) => [
      k,
      num(val as number | string),
    ]),
  );
}

function mapCoordMatrix(v: unknown): HexCoordinate[][] {
  if (!Array.isArray(v)) return [];
  return v.map((row) =>
    Array.isArray(row) ? row.map((c) => toHexCoord(c as HexCoordinateDto)) : [],
  );
}

function mapPrivateGameFromPayload(root: Record<string, unknown>): PrivateGameInfo | null {
  const raw = root.myPrivateGameInfo;
  if (!raw || typeof raw !== "object") return null;
  const o = raw as Record<string, unknown>;
  const myPlayerId =
    o.myPlayerId != null ? String(o.myPlayerId as string | number) : "";
  if (!myPlayerId) return null;

  const handRaw = o.myHand;
  if (!handRaw || typeof handRaw !== "object") return null;
  const h = handRaw as Record<string, unknown>;
  const myHand: MyHand = {
    resources: mapStringNumDict(h.resources ?? h.Resources),
    devCards: mapStringNumDict(h.devCards ?? h.DevCards),
    buildables: mapStringNumDict(h.buildables ?? h.Buildables),
  };

  const roadLists = o.buildableRoads ?? o.BuildableRoads;
  const settleLists = o.buildableSettlements ?? o.BuildableSettlements;

  return {
    myPlayerId,
    myHand,
    myScore: num(o.myScore as number | undefined),
    buildableRoads: mapCoordMatrix(roadLists),
    buildableSettlements: mapCoordMatrix(settleLists),
  };
}

function publicDtoToGame(publicPart: PublicGameDto): Game | null {
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
    gameType: normalizeGameType(publicPart.gameType),
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
    playerDirection: normalizePlayerDirection(publicPart.playerDirection),
    gamePhase: normalizeGamePhase(publicPart.gamePhase),
    round: num(publicPart.round),
    playerIndex: Number.isFinite(playerIndex) && playerIndex >= 0 ? playerIndex : 0,
    currentTradeOffer: mapTradeOffer(publicPart.currentTradeOffer),
    players,
  };
}

/** Public slice only (ignores `myPrivateGameInfo`). */
export function mapPublicGameFromPayload(payload: unknown): Game | null {
  const dto = extractPublicGameDto(payload);
  return dto ? publicDtoToGame(dto) : null;
}

/**
 * Full projection: public `game` plus optional `myPrivateGameInfo` (per-user SignalR / GameDto).
 */
export function mapGamePayload(payload: unknown): {
  game: Game;
  privateGame: PrivateGameInfo | null;
} | null {
  if (!isRecord(payload)) return null;
  const publicPart = extractPublicGameDto(payload);
  if (!publicPart) return null;

  const game = publicDtoToGame(publicPart);
  if (!game) return null;

  const privateGame = mapPrivateGameFromPayload(payload);
  return { game, privateGame };
}

/**
 * @deprecated Use {@link mapPublicGameFromPayload} — this name implied the full DTO. Returns public `Game` only.
 */
export function gamePayloadToDomain(payload: unknown): Game | null {
  return mapPublicGameFromPayload(payload);
}
