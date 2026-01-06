export type Board = {
  hexes: Hex[];
  populationCenters: PopulationCenter[];
  roads: Road[];
  ports: Port[];
};

export type Hex = {
  coordinate: HexCoordinate;
  resource: string;
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
  coordinates: HexCoordinate[];
  type: string;
};

export type HexCoordinate = {
  q: number;
  r: number;
  s: number;
};
