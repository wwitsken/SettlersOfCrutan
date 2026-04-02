export type PlayerColor =
  | "none"
  | "red"
  | "blue"
  | "white"
  | "orange"
  | "green"
  | "yellow"
  | "brown"
  | "purple";

export interface Player {
  id: string;
  name: string;
  color: PlayerColor;
  vp: number;
  cardCount: number;
  devCardCount: number;
  isCurrentTurn: boolean;
}

export type ResourceType = "wood" | "brick" | "sheep" | "wheat" | "ore";
export type DevCardType =
  | "knight"
  | "monopoly"
  | "road_building"
  | "year_of_plenty"
  | "vp";

export interface ResourceHand {
  wood: number;
  brick: number;
  sheep: number;
  wheat: number;
  ore: number;
}

export interface UnplayedDevCard {
  id: string;
  type: DevCardType;
}

export interface PlayedDevCards {
  knight: number;
  monopoly: number;
  road_building: number;
  year_of_plenty: number;
  vp: number;
}

export interface ChatMessage {
  id: number;
  player: string;
  color: PlayerColor;
  text: string;
  time: string;
}
