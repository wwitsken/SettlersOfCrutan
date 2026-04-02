import type { Game } from "./game";
import type { Player } from "./player";
import type { PrivateGameInfo } from "./privateGame";

/** Public `Player` row for the current user when `privateGame` is present. */
export function getCurrentPlayer(
  game: Game,
  privateGame: PrivateGameInfo | null,
): Player | undefined {
  if (!privateGame?.myPlayerId) return undefined;
  return game.players.find((p) => p.id === privateGame.myPlayerId);
}

/** Sidebar player list: domain fields plus `isCurrentTurn` from `game.playerIndex`. */
export function playersForLayout(game: Game): (Player & { isCurrentTurn: boolean })[] {
  const currentId = game.players[game.playerIndex]?.id;
  return game.players.map((p) => ({
    ...p,
    isCurrentTurn: p.id === currentId,
  }));
}
