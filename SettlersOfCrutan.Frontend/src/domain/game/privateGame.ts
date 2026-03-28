import type { HexCoordinate } from "./board";

export type MyHand = {
  resources: Record<string, number>;
  devCards: Record<string, number>;
  buildables: Record<string, number>;
};

/** Per-user slice from `GameDto.myPrivateGameInfo` (SignalR / player-scoped APIs). */
export type PrivateGameInfo = {
  myPlayerId: string;
  myHand: MyHand;
  myScore: number;
  buildableRoads: HexCoordinate[][];
  buildableSettlements: HexCoordinate[][];
};
