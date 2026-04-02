import type { PlayerColor } from "../domain/game/gameTypes";

export interface ChatMessage {
  id: number;
  player: string;
  color: PlayerColor;
  text: string;
  time: string;
}
