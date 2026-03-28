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
