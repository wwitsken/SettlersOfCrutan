import type { Board } from "./board";
import type { Player } from "./player";
import type { TradeOffer } from "./tradeOffer";
import type {
  CurrentDiceRoll,
  GamePhase,
  GameType,
  PlayerDirection,
} from "./gameTypes";

export type Game = {
  id: string;
  gameType: GameType;
  gameName: string | undefined;
  board: Board;
  bankResourceHand: Record<string, number>;
  bankDevCardHand: Record<string, number>;
  turnExpiresAt: Date | undefined;
  playerDirection: PlayerDirection;
  gamePhase: GamePhase;
  round: number;
  playerIndex: number;
  currentTradeOffer: TradeOffer | undefined;
  players: Player[];
  /** Set once `gamePhase === "gameEnd"`. Lets reload / late-join clients resolve the winner without SignalR. */
  winnerPlayerId?: string;
  currentDiceRoll?: CurrentDiceRoll;
};
