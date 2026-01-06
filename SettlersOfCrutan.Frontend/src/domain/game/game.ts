import type { Board } from "./board";
import type { Player } from "./player";
import type { TradeOffer } from "./tradeOffer";

export type Game = {
  id: string;
  gameType: string;
  gameName: string | undefined;
  board: Board;
  bankResourceHand: Record<string, number>;
  bankDevCardHand: Record<string, number>;
  turnExpiresAt: Date | undefined;
  playerDirection: string;
  gamePhase: string;
  round: number;
  playerIndex: number;
  currentTradeOffer: TradeOffer | undefined;
  players: Player[];
};
