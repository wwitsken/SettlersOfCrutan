import type { components } from "../../api/types";

/** OpenAPI-aligned enums for domain models (no separate adapter DTO layer). */
export type ResourceCardType = components["schemas"]["ResourceCardType"];
export type PlayerColor = components["schemas"]["PlayerColor"];
export type GamePhase = components["schemas"]["GamePhase"];
export type PlayerDirection = components["schemas"]["PlayerDirection"];
export type GameType = components["schemas"]["GameType"];
export type DevelopmentCardType = components["schemas"]["DevelopmentCardType"];
export type CurrentDiceRoll = {
    die1: number,
    die2: number
}

export interface ChatMessage {
  id: string | number;
  player: string;
  color: PlayerColor;
  text: string;
  time: string;
}
