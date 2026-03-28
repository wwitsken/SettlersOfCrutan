import { useCallback, useState } from "react";
import type { components } from "../api/types";
import type { HexCoordinate } from "../domain/game/board";

export type GamePageInteractionMode =
  | "idle"
  | "buildRoad"
  | "buildSettlement"
  | "upgradeCity"
  | "robberHex"
  | "devKnightHex"
  | "initialSettle"
  | "initialRoad"
  | "devRoad1"
  | "devRoad2";

export type BoardPickMode = "none" | "road" | "settlement" | "cityUpgrade" | "robberHex";

type VertexCoordDto = components["schemas"]["VertexCoordDto"];
type EdgeCoordDto = components["schemas"]["EdgeCoordDto"];

export function boardPickModeFromInteraction(
  mode: GamePageInteractionMode,
): BoardPickMode {
  switch (mode) {
    case "buildRoad":
    case "initialRoad":
    case "devRoad1":
    case "devRoad2":
      return "road";
    case "buildSettlement":
    case "initialSettle":
      return "settlement";
    case "upgradeCity":
      return "cityUpgrade";
    case "robberHex":
    case "devKnightHex":
      return "robberHex";
    default:
      return "none";
  }
}

export function useGamePageInteraction() {
  const [interactionMode, setInteractionMode] =
    useState<GamePageInteractionMode>("idle");
  const [pendingInitialVertex, setPendingInitialVertex] =
    useState<VertexCoordDto | null>(null);
  const [pendingRobberHex, setPendingRobberHex] =
    useState<HexCoordinate | null>(null);
  const [robberKind, setRobberKind] = useState<"resolve" | "knight" | null>(
    null,
  );
  const [devRoadFirstEdge, setDevRoadFirstEdge] =
    useState<EdgeCoordDto | null>(null);
  const [actionError, setActionError] = useState<string | null>(null);

  const clearRobberPending = useCallback(() => {
    setPendingRobberHex(null);
    setRobberKind(null);
  }, []);

  const resetDevRoad = useCallback(() => {
    setDevRoadFirstEdge(null);
    setInteractionMode("idle");
  }, []);

  return {
    interactionMode,
    setInteractionMode,
    pendingInitialVertex,
    setPendingInitialVertex,
    pendingRobberHex,
    setPendingRobberHex,
    robberKind,
    setRobberKind,
    devRoadFirstEdge,
    setDevRoadFirstEdge,
    actionError,
    setActionError,
    clearRobberPending,
    resetDevRoad,
  };
}
