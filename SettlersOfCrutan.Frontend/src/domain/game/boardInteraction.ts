import type { Game } from "./game";
import type { HexCoordinate } from "./board";
import type { PrivateGameInfo } from "./privateGame";

export type BoardPickMode = "none" | "build" | "moveRobber";

export type RobberCommandContext = "resolve" | "knight" | null;

export type DerivedBoardInteraction = {
  boardPickMode: BoardPickMode;
  /** True when the player may pick a hex for robber (dice resolve or post-knight). */
  allowRobberHexPick: boolean;
  /** How to attribute `postResolveRobber` when finishing robber flow. */
  robberCommandContext: RobberCommandContext;
  showGhostRoads: boolean;
  showGhostSettlementVertices: boolean;
  showGhostCityUpgrades: boolean;
  buildableRoads?: HexCoordinate[][];
  buildableSettlements?: HexCoordinate[][];
  initialRoadVertexHexes?: HexCoordinate[] | null;
};

type Args = {
  boardViewLive: boolean;
  game: Game | null;
  privateGame: PrivateGameInfo | null;
  isMyTurn: boolean;
  pendingInitialVertex: unknown | null;
  pendingRobberHex: HexCoordinate | null;
  awaitingKnightRobberHex: boolean;
  devRoadPicking: boolean;
  initialRoadVertexHexes: HexCoordinate[] | null;
};

/**
 * Derives board pick mode, ghost layers, and buildable filters from game phase
 * plus minimal local state (setup vertex, dev roads, knight robber).
 */
export function deriveBoardInteraction(a: Args): DerivedBoardInteraction {
  const none: DerivedBoardInteraction = {
    boardPickMode: "none",
    allowRobberHexPick: false,
    robberCommandContext: null,
    showGhostRoads: false,
    showGhostSettlementVertices: false,
    showGhostCityUpgrades: false,
    buildableRoads: undefined,
    buildableSettlements: undefined,
    initialRoadVertexHexes: null,
  };

  if (!a.boardViewLive || !a.game || !a.privateGame || !a.isMyTurn) {
    return none;
  }

  const phase = a.game.gamePhase;
  const pg = a.privateGame;

  if (a.pendingRobberHex) {
    return none;
  }

  const resolveRobberActive = phase === "resolveRobber";
  if (resolveRobberActive) {
    return {
      boardPickMode: "moveRobber",
      allowRobberHexPick: true,
      robberCommandContext: "resolve",
      showGhostRoads: false,
      showGhostSettlementVertices: false,
      showGhostCityUpgrades: false,
      buildableRoads: undefined,
      buildableSettlements: undefined,
      initialRoadVertexHexes: null,
    };
  }

  if (a.awaitingKnightRobberHex) {
    return {
      boardPickMode: "moveRobber",
      allowRobberHexPick: true,
      robberCommandContext: "knight",
      showGhostRoads: false,
      showGhostSettlementVertices: false,
      showGhostCityUpgrades: false,
      buildableRoads: undefined,
      buildableSettlements: undefined,
      initialRoadVertexHexes: null,
    };
  }

  if (a.devRoadPicking) {
    return {
      boardPickMode: "build",
      allowRobberHexPick: false,
      robberCommandContext: null,
      showGhostRoads: true,
      showGhostSettlementVertices: false,
      showGhostCityUpgrades: false,
      buildableRoads: undefined,
      buildableSettlements: undefined,
      initialRoadVertexHexes: null,
    };
  }

  if (phase === "setup") {
    const awaitingRoad = a.pendingInitialVertex != null;
    return {
      boardPickMode: "build",
      allowRobberHexPick: false,
      robberCommandContext: null,
      showGhostRoads: awaitingRoad,
      showGhostSettlementVertices: !awaitingRoad,
      showGhostCityUpgrades: false,
      buildableRoads: awaitingRoad ? undefined : undefined,
      buildableSettlements: awaitingRoad ? undefined : pg.buildableSettlements,
      initialRoadVertexHexes: awaitingRoad ? a.initialRoadVertexHexes : null,
    };
  }

  if (phase === "tradeBuild") {
    return {
      boardPickMode: "build",
      allowRobberHexPick: false,
      robberCommandContext: null,
      showGhostRoads: true,
      showGhostSettlementVertices: true,
      showGhostCityUpgrades: true,
      buildableRoads: pg.buildableRoads,
      buildableSettlements: pg.buildableSettlements,
      initialRoadVertexHexes: null,
    };
  }

  return none;
}
