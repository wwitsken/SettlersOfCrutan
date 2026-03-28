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
  /** Public VP from settlements/cities; excludes hidden VP dev cards. */
  victoryPoints: number;
  hasLongestRoad: boolean;
  hasLargestArmy: boolean;
};
