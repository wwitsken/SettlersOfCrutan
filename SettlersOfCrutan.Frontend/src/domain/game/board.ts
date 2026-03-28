import type { ResourceCardType } from "./gameTypes";

export type Board = {
  hexes: Hex[];
  populationCenters: PopulationCenter[];
  roads: Road[];
  ports: Port[];
};

export type Hex = {
  coordinate: HexCoordinate;
  resource: ResourceCardType;
  numberToken: number;
  hasRobber: boolean;
};

export type PopulationCenter = {
  coordinates: HexCoordinate[];
  type: "settlement" | "city";
  playerOwnerId: string;
};

export type Road = {
  coordinates: HexCoordinate[];
  playerOwnerId: string;
};

export type Port = {
  inCoordinate: HexCoordinate;
  outCoordinate: HexCoordinate;
  type: string;
};

export type HexCoordinate = {
  q: number;
  r: number;
  s: number;
};
