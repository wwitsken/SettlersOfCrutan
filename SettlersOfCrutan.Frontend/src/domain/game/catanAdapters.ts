import type { Game } from "./game";
import type { PlayerColor as DomainPlayerColor } from "./gameTypes";
import type {
  DevCardType,
  PlayedDevCards,
  Player as MigrationPlayer,
  PlayerColor as MigrationPlayerColor,
  ResourceHand,
  UnplayedDevCard,
} from "../../types/catan";

// API resource key → migration resource key
const RESOURCE_KEY_MAP: Record<string, keyof ResourceHand | undefined> = {
  lumber: "wood",
  wool: "sheep",
  grain: "wheat",
  brick: "brick",
  ore: "ore",
};

// API dev card key → migration dev card key
const DEV_KEY_MAP: Record<string, DevCardType | undefined> = {
  knight: "knight",
  monopoly: "monopoly",
  roadBuilding: "road_building",
  yearOfPlenty: "year_of_plenty",
  victoryPoint: "vp",
};

export function mapResourceHand(resources: Record<string, number>): ResourceHand {
  const hand: ResourceHand = { wood: 0, brick: 0, sheep: 0, wheat: 0, ore: 0 };
  for (const [apiKey, count] of Object.entries(resources)) {
    const migKey = RESOURCE_KEY_MAP[apiKey];
    if (migKey) hand[migKey] = count;
  }
  return hand;
}

export function mapUnplayedDevCards(devCards: Record<string, number>): UnplayedDevCard[] {
  const result: UnplayedDevCard[] = [];
  for (const [apiKey, count] of Object.entries(devCards)) {
    const migType = DEV_KEY_MAP[apiKey];
    if (!migType) continue;
    for (let i = 0; i < count; i++) {
      result.push({ id: `${apiKey}-${i}`, type: migType });
    }
  }
  return result;
}

export function mapPlayedDevCards(): PlayedDevCards {
  return { knight: 0, monopoly: 0, road_building: 0, year_of_plenty: 0, vp: 0 };
}

// Domain PlayerColor is already a superset of migration PlayerColor after widening,
// so this is a direct cast. The function keeps the call site type-safe.
export function mapPlayerColor(color: DomainPlayerColor): MigrationPlayerColor {
  return color as MigrationPlayerColor;
}

export function mapGamePlayers(game: Game): MigrationPlayer[] {
  const currentId = game.players[game.playerIndex]?.id;
  return game.players.map((p) => ({
    id: p.id,
    name: p.displayName,
    color: mapPlayerColor(p.playerColor),
    vp: p.victoryPoints,
    cardCount: p.resourceCardCount,
    devCardCount: p.developmentCardCount,
    isCurrentTurn: p.id === currentId,
  }));
}
