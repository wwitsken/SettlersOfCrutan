export type Player = {
  id: string;
  playOrder: number;
  isPlaying: boolean;
  displayName: string;
  playerColor: string;
  resourceCardCount: number;
  developmentCardCount: number;
  pieceReserve: Record<string, number>;
  discardRequirement: number;
};
