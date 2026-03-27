import type { Game } from "./game";

export const game: Game = {
  id: "123",
  gameType: "mine",
  gameName: "this one",
  board: {
    hexes: [
      {
        coordinate: { q: 0, r: 0, s: 0 },
        resource: "wood",
        numberToken: 5,
        hasRobber: false,
      },
      {
        coordinate: { q: 1, r: -1, s: 0 },
        resource: "wheat",
        numberToken: 8,
        hasRobber: false,
      },
      {
        coordinate: { q: 0, r: -1, s: 1 },
        resource: "brick",
        numberToken: 8,
        hasRobber: false,
      },
    ],
    populationCenters: [
      {
        coordinates: [
          { q: 0, r: 0, s: 0 },
          { q: 1, r: -1, s: 0 },
          { q: 0, r: -1, s: 1 },
        ],
        type: "settlement",
        playerOwnerId: "p1",
      },
      {
        coordinates: [
          { q: 0, r: 1, s: -1 },
          { q: 0, r: 0, s: 0 },
          { q: 1, r: 0, s: -1 },
        ],
        type: "city",
        playerOwnerId: "p1",
      },
    ],
    roads: [
      {
        coordinates: [
          { q: 1, r: -1, s: 0 },
          { q: 0, r: 0, s: 0 },
        ],
        playerOwnerId: "p1",
      },
      {
        coordinates: [
          { q: 1, r: 0, s: -1 },
          { q: 0, r: 0, s: 0 },
        ],
        playerOwnerId: "p1",
      },
      {
        coordinates: [
          { q: 0, r: 0, s: 0 },
          { q: 0, r: -1, s: 1 },
        ],
        playerOwnerId: "p1",
      },
    ],
    ports: [
      {
        // Provide two coordinates so the port can be centered on the edge midpoint (like roads)
        inCoordinate: { q: 1, r: -1, s: 0 },
        outCoordinate: { q: 1, r: -2, s: 1 },
        type: "generic",
      },
    ],
  },
  bankResourceHand: { wood: 0, brick: 0, sheep: 0, wheat: 0, ore: 0 },
  bankDevCardHand: {
    knight: 0,
    monopoly: 0,
    roadBuilding: 0,
    yearOfPlenty: 0,
    victoryPoint: 0,
  },
  turnExpiresAt: undefined,
  playerDirection: "clockwise",
  gamePhase: "setup",
  round: 1,
  playerIndex: 0,
  currentTradeOffer: undefined,
  players: [
    {
      id: "p1",
      playOrder: 1,
      isPlaying: true,
      displayName: "Alice",
      playerColor: "red",
      resourceCardCount: 0,
      developmentCardCount: 0,
      pieceReserve: { settlement: 5, city: 4, road: 15 },
      discardRequirement: 0,
    },
  ],
};
