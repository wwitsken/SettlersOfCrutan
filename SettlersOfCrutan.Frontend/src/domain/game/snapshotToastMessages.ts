import type { Game } from "./game";
import type { Board, HexCoordinate, PopulationCenter, Road } from "./board";
import type { PrivateGameInfo } from "./privateGame";
import type { GamePhase } from "./gameTypes";

function coordKey(c: HexCoordinate): string {
  return `${c.q},${c.r},${c.s}`;
}

function playerName(game: Game, playerId: string): string {
  return game.players.find((p) => p.id === playerId)?.displayName ?? "Player";
}

function robberLocation(board: Board): string | null {
  const hex = board.hexes.find((h) => h.hasRobber);
  return hex ? coordKey(hex.coordinate) : null;
}

function roadEdgeKey(r: Road): string | null {
  if (r.coordinates.length < 2) return null;
  const a = coordKey(r.coordinates[0]!);
  const b = coordKey(r.coordinates[1]!);
  return a < b ? `${a}|${b}` : `${b}|${a}`;
}

function roadKeys(board: Board): Map<string, string> {
  const m = new Map<string, string>();
  for (const road of board.roads) {
    const k = roadEdgeKey(road);
    if (k) m.set(k, road.playerOwnerId);
  }
  return m;
}

function pcLocationKey(pc: PopulationCenter): string {
  return [...pc.coordinates].map(coordKey).sort().join("|");
}

function populationByLocation(
  board: Board,
): Map<string, { type: PopulationCenter["type"]; ownerId: string }> {
  const m = new Map<
    string,
    { type: PopulationCenter["type"]; ownerId: string }
  >();
  for (const pc of board.populationCenters) {
    m.set(pcLocationKey(pc), { type: pc.type, ownerId: pc.playerOwnerId });
  }
  return m;
}

function sumBankDev(game: Game): number {
  return Object.values(game.bankDevCardHand).reduce((a, n) => a + (n ?? 0), 0);
}

function summarizeTradeResources(rec: Record<string, number>): string {
  const parts = Object.entries(rec)
    .filter(([, n]) => (n ?? 0) > 0)
    .map(([k, n]) => `${n}× ${k}`);
  return parts.length ? parts.join(", ") : "nothing";
}

function phaseToast(
  prev: GamePhase,
  next: GamePhase,
  nextGame: Game,
): string | null {
  const name = playerName(
    nextGame,
    nextGame.players[nextGame.playerIndex]?.id ?? "",
  );
  if (prev === "rollDice" && next === "tradeBuild")
    return "Dice rolled — trade & build phase.";
  if (prev === "rollDice" && next === "discardHalf")
    return "Dice rolled — discards required.";
  if (prev === "rollDice" && next === "resolveRobber")
    return "Robber activates — pick where to move it.";
  if (prev === "resolveRobber" && next === "tradeBuild")
    return "Robber moved — trade & build phase.";
  if (prev === "tradeBuild" && next === "rollDice")
    return name ? `${name}'s turn — roll the dice.` : "Roll the dice.";
  if (prev === "tradeBuild" && next === "resolveRobber")
    return "Robber must move — choose a new hex.";
  if (next === "gameEnd") return "Game over.";
  if (prev !== "setup" && next === "setup") return "Setup phase.";
  if (prev === "setup" && next === "rollDice")
    return name
      ? `${name} finishes setup — roll the dice.`
      : "Setup complete — roll the dice.";
  return null;
}

function turnHandoffToast(prev: Game, next: Game): string | null {
  if (prev.gamePhase !== "tradeBuild" || next.gamePhase !== "tradeBuild")
    return null;
  if (prev.playerIndex === next.playerIndex) return null;
  const name = playerName(next, next.players[next.playerIndex]?.id ?? "");
  return name ? `${name}'s turn.` : "Next player's turn.";
}

/**
 * Human-readable lines for UI toasts from two full game snapshots (HTTP / SignalR).
 * Skips the first load when `prev` is null or game id changes.
 */
export function snapshotToastMessages(
  prev: Game | null,
  next: Game,
  nextPrivate: PrivateGameInfo | null,
): string[] {
  if (!prev || prev.id !== next.id) return [];

  const out: string[] = [];
  const push = (s: string) => {
    if (!out.includes(s)) out.push(s);
  };

  if (prev.gamePhase !== next.gamePhase) {
    const p = phaseToast(prev.gamePhase, next.gamePhase, next);
    if (p) push(p);
  }

  const th = turnHandoffToast(prev, next);
  if (th) push(th);

  const prevRob = robberLocation(prev.board);
  const nextRob = robberLocation(next.board);
  if (prevRob !== nextRob && nextRob !== null) {
    const mover = playerName(next, next.players[next.playerIndex]?.id ?? "");
    push(
      mover
        ? `Robber moved (${mover}'s action).`
        : "Robber moved to a new hex.",
    );
  }

  const prevRoads = roadKeys(prev.board);
  const nextRoads = roadKeys(next.board);
  for (const [edge, ownerId] of nextRoads) {
    if (!prevRoads.has(edge)) {
      push(`${playerName(next, ownerId)} built a road.`);
    }
  }

  const prevPop = populationByLocation(prev.board);
  const nextPop = populationByLocation(next.board);
  for (const [loc, after] of nextPop) {
    const before = prevPop.get(loc);
    if (!before && after.type === "settlement") {
      push(`${playerName(next, after.ownerId)} built a settlement.`);
    } else if (
      before?.type === "settlement" &&
      after.type === "city" &&
      before.ownerId === after.ownerId
    ) {
      push(`${playerName(next, after.ownerId)} upgraded to a city.`);
    }
  }

  const prevOffer = prev.currentTradeOffer;
  const nextOffer = next.currentTradeOffer;

  if (
    prevOffer &&
    nextOffer &&
    prevOffer.id === nextOffer.id &&
    nextOffer.isAccepted &&
    !prevOffer.isAccepted
  ) {
    push("Trade accepted.");
  }

  if (nextOffer && !nextOffer.isAccepted) {
    const isNew = !prevOffer || prevOffer.id !== nextOffer.id;
    if (isNew) {
      if (prevOffer && !prevOffer.isAccepted && prevOffer.id !== nextOffer.id) {
        push("Trade offer closed.");
      }
      const proposer = playerName(next, nextOffer.playerProposerId);
      const give = summarizeTradeResources(nextOffer.offeredResources);
      const want = summarizeTradeResources(nextOffer.requestedResources);
      push(`${proposer} proposes a trade: offer ${give} for ${want}.`);
    }
  }

  if (prevOffer && !prevOffer.isAccepted && !nextOffer) {
    push("Trade offer closed.");
  }

  const prevDevSum = sumBankDev(prev);
  const nextDevSum = sumBankDev(next);
  if (nextDevSum < prevDevSum) {
    for (const p of next.players) {
      const prevP = prev.players.find((x) => x.id === p.id);
      const before = prevP?.developmentCardCount ?? 0;
      if (p.developmentCardCount === before + 1) {
        const me = nextPrivate?.myPlayerId;
        if (me && p.id === me) push("You bought a development card.");
        else push(`${playerName(next, p.id)} bought a development card.`);
        break;
      }
    }
  }

  return out;
}
