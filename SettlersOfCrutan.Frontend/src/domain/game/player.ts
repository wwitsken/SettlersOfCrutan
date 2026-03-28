import type { PlayerColor } from "./gameTypes";

export type Player = {
  id: string;
  playOrder: number;
  isPlaying: boolean;
  displayName: string;
  playerColor: PlayerColor;
  resourceCardCount: number;
  developmentCardCount: number;
  pieceReserve: Record<string, number>;
  discardRequirement: number;
};
