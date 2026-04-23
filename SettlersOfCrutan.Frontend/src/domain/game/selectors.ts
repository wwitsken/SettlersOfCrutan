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

export type MeAndTurn = {
  me: Player | undefined;
  currentPlayerId: string | undefined;
  isMyTurn: boolean;
};

/**
 * Resolve the "who am I / whose turn is it" trio from a game snapshot plus
 * the private slice. `me` follows the enrichment state of whatever `game` is
 * passed in; `currentPlayerId` and `isMyTurn` only depend on player ids.
 */
export function selectMeAndTurn(
  game: Game | null,
  privateGame: PrivateGameInfo | null,
): MeAndTurn {
  const me = game && privateGame ? getCurrentPlayer(game, privateGame) : undefined;
  const currentPlayerId =
    game && game.players.length > 0 ? game.players[game.playerIndex]?.id : undefined;
  const isMyTurn = !!(
    privateGame &&
    currentPlayerId &&
    currentPlayerId === privateGame.myPlayerId
  );
  return { me, currentPlayerId, isMyTurn };
}
