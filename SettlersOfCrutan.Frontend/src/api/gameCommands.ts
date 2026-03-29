import { api } from "./client";
import { acquireAccessToken } from "../authConfig";
import type { components } from "./types";

type EdgeCoordDto = components["schemas"]["EdgeCoordDto"];
type VertexCoordDto = components["schemas"]["VertexCoordDto"];
type HexCoordDto = components["schemas"]["HexCoordDto"];
type ResourceCardAmountDto = components["schemas"]["ResourceCardAmountDto"];
type ResourceCardType = components["schemas"]["ResourceCardType"];

export type CommandResult = { ok: true } | { ok: false; errorMessage: string };

function problemMessage(
  status: number,
  body: unknown,
): string {
  if (body && typeof body === "object") {
    const o = body as { title?: string; detail?: string };
    if (o.detail) return o.detail;
    if (o.title) return o.title;
  }
  return `Request failed (${status})`;
}

async function tokenOrErr(): Promise<string | CommandResult> {
  try {
    return await acquireAccessToken();
  } catch {
    return { ok: false, errorMessage: "Not signed in." };
  }
}

export async function postRollDice(gameId: string): Promise<CommandResult> {
  const t = await tokenOrErr();
  if (typeof t !== "string") return t;
  const { error, response } = await api.POST("/api/games/{id}/play/roll-dice", {
    params: { path: { id: gameId } },
    accessToken: t,
  });
  if (response.status === 200 && !error) return { ok: true };
  return { ok: false, errorMessage: problemMessage(response.status, error) };
}

export async function postPlaceInitial(
  gameId: string,
  settlementVertexCoordinate: VertexCoordDto,
  roadEdgeCoordinate: EdgeCoordDto,
): Promise<CommandResult> {
  const t = await tokenOrErr();
  if (typeof t !== "string") return t;
  const { error, response } = await api.POST("/api/games/{id}/play/place-initial", {
    params: { path: { id: gameId } },
    body: { settlementVertexCoordinate, roadEdgeCoordinate },
    accessToken: t,
  });
  if (response.status === 204 && !error) return { ok: true };
  return { ok: false, errorMessage: problemMessage(response.status, error) };
}

export async function postBuildRoad(
  gameId: string,
  edgeCoordinate: EdgeCoordDto,
): Promise<CommandResult> {
  const t = await tokenOrErr();
  if (typeof t !== "string") return t;
  const { error, response } = await api.POST("/api/games/{id}/build/road", {
    params: { path: { id: gameId } },
    body: { edgeCoordinate },
    accessToken: t,
  });
  if (response.status === 204 && !error) return { ok: true };
  return { ok: false, errorMessage: problemMessage(response.status, error) };
}

export async function postBuildSettlement(
  gameId: string,
  vertexCoordinate: VertexCoordDto,
): Promise<CommandResult> {
  const t = await tokenOrErr();
  if (typeof t !== "string") return t;
  const { error, response } = await api.POST("/api/games/{id}/build/settlement", {
    params: { path: { id: gameId } },
    body: { vertexCoordinate },
    accessToken: t,
  });
  if (response.status === 204 && !error) return { ok: true };
  return { ok: false, errorMessage: problemMessage(response.status, error) };
}

export async function postUpgradeCity(
  gameId: string,
  vertexCoordinate: VertexCoordDto,
): Promise<CommandResult> {
  const t = await tokenOrErr();
  if (typeof t !== "string") return t;
  const { error, response } = await api.POST("/api/games/{id}/build/city", {
    params: { path: { id: gameId } },
    body: { vertexCoordinate },
    accessToken: t,
  });
  if (response.status === 204 && !error) return { ok: true };
  return { ok: false, errorMessage: problemMessage(response.status, error) };
}

export async function postBuyDevelopmentCard(gameId: string): Promise<CommandResult> {
  const t = await tokenOrErr();
  if (typeof t !== "string") return t;
  const { error, response } = await api.POST(
    "/api/games/{id}/build/development-card",
    {
      params: { path: { id: gameId } },
      accessToken: t,
    },
  );
  if (response.status === 200 && !error) return { ok: true };
  return { ok: false, errorMessage: problemMessage(response.status, error) };
}

export async function postEndTurn(gameId: string): Promise<CommandResult> {
  const t = await tokenOrErr();
  if (typeof t !== "string") return t;
  const { error, response } = await api.POST("/api/games/{id}/turn/end", {
    params: { path: { id: gameId } },
    accessToken: t,
  });
  if (response.status === 200 && !error) return { ok: true };
  return { ok: false, errorMessage: problemMessage(response.status, error) };
}

export async function postDiscardHalf(
  gameId: string,
  discards: ResourceCardAmountDto[],
): Promise<CommandResult> {
  const t = await tokenOrErr();
  if (typeof t !== "string") return t;
  const { error, response } = await api.POST("/api/games/{id}/turn/discard-half", {
    params: { path: { id: gameId } },
    body: { discards },
    accessToken: t,
  });
  if (response.status === 204 && !error) return { ok: true };
  return { ok: false, errorMessage: problemMessage(response.status, error) };
}

export async function postResolveRobber(
  gameId: string,
  newRobberHex: HexCoordDto,
  victimPlayerId?: string | null,
): Promise<CommandResult> {
  const t = await tokenOrErr();
  if (typeof t !== "string") return t;
  const body: { newRobberHex: HexCoordDto; victimPlayerId?: string } = {
    newRobberHex,
  };
  if (
    victimPlayerId !== undefined &&
    victimPlayerId !== null &&
    victimPlayerId !== ""
  ) {
    body.victimPlayerId = victimPlayerId;
  }
  const { error, response } = await api.POST("/api/games/{id}/turn/resolve-robber", {
    params: { path: { id: gameId } },
    body,
    accessToken: t,
  });
  if (response.status === 200 && !error) return { ok: true };
  return { ok: false, errorMessage: problemMessage(response.status, error) };
}

export async function postUseKnight(gameId: string): Promise<CommandResult> {
  const t = await tokenOrErr();
  if (typeof t !== "string") return t;
  const { error, response } = await api.POST("/api/games/{id}/devcards/knight", {
    params: { path: { id: gameId } },
    accessToken: t,
  });
  if (response.status === 204 && !error) return { ok: true };
  return { ok: false, errorMessage: problemMessage(response.status, error) };
}

export async function postUseMonopoly(
  gameId: string,
  playerId: string,
  resourceType: ResourceCardType,
): Promise<CommandResult> {
  const t = await tokenOrErr();
  if (typeof t !== "string") return t;
  const { error, response } = await api.POST("/api/games/{id}/devcards/monopoly", {
    params: { path: { id: gameId } },
    body: { playerId, resourceType },
    accessToken: t,
  });
  if (response.status === 200 && !error) return { ok: true };
  return { ok: false, errorMessage: problemMessage(response.status, error) };
}

export async function postUseYearOfPlenty(
  gameId: string,
  playerId: string,
  resource1: ResourceCardType,
  resource2: ResourceCardType,
): Promise<CommandResult> {
  const t = await tokenOrErr();
  if (typeof t !== "string") return t;
  const { error, response } = await api.POST(
    "/api/games/{id}/devcards/year-of-plenty",
    {
      params: { path: { id: gameId } },
      body: { playerId, resource1, resource2 },
      accessToken: t,
    },
  );
  if (response.status === 200 && !error) return { ok: true };
  return { ok: false, errorMessage: problemMessage(response.status, error) };
}

export async function postUseRoadBuilding(
  gameId: string,
  playerId: string,
  edge1: EdgeCoordDto,
  edge2: EdgeCoordDto,
): Promise<CommandResult> {
  const t = await tokenOrErr();
  if (typeof t !== "string") return t;
  const { error, response } = await api.POST(
    "/api/games/{id}/devcards/road-building",
    {
      params: { path: { id: gameId } },
      body: { playerId, edge1, edge2 },
      accessToken: t,
    },
  );
  if (response.status === 204 && !error) return { ok: true };
  return { ok: false, errorMessage: problemMessage(response.status, error) };
}
