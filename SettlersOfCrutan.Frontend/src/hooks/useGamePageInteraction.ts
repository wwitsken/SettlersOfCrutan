import { useCallback, useState } from "react";
import type { components } from "../api/types";
import type { HexCoordinate } from "../domain/game/board";

export type { BoardPickMode } from "../domain/game/boardInteraction";

type VertexCoordDto = components["schemas"]["VertexCoordDto"];
type EdgeCoordDto = components["schemas"]["EdgeCoordDto"];

export type DevRoadFlow = "idle" | "pickFirst" | "pickSecond";

export function useGamePageInteraction() {
  const [pendingInitialVertex, setPendingInitialVertex] =
    useState<VertexCoordDto | null>(null);
  const [pendingRobberHex, setPendingRobberHex] =
    useState<HexCoordinate | null>(null);
  const [awaitingKnightRobberHex, setAwaitingKnightRobberHex] = useState(false);
  const [devRoadFlow, setDevRoadFlow] = useState<DevRoadFlow>("idle");
  const [devRoadFirstEdge, setDevRoadFirstEdge] =
    useState<EdgeCoordDto | null>(null);
  const [actionError, setActionError] = useState<string | null>(null);

  const clearRobberPending = useCallback(() => {
    setPendingRobberHex(null);
    setAwaitingKnightRobberHex(false);
  }, []);

  const resetDevRoad = useCallback(() => {
    setDevRoadFirstEdge(null);
    setDevRoadFlow("idle");
  }, []);

  const startDevRoadFlow = useCallback(() => {
    setDevRoadFirstEdge(null);
    setDevRoadFlow("pickFirst");
  }, []);

  return {
    pendingInitialVertex,
    setPendingInitialVertex,
    pendingRobberHex,
    setPendingRobberHex,
    awaitingKnightRobberHex,
    setAwaitingKnightRobberHex,
    devRoadFlow,
    setDevRoadFlow,
    devRoadFirstEdge,
    setDevRoadFirstEdge,
    devRoadPicking: devRoadFlow !== "idle",
    actionError,
    setActionError,
    clearRobberPending,
    resetDevRoad,
    startDevRoadFlow,
  };
}
