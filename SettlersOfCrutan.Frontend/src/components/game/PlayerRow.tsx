import type { Player } from "../../domain/game/player";
import { COLOR_MAP } from "../../constants/catanMeta";

type Props = {
  player: Player;
  isCurrentTurn: boolean;
};

export default function PlayerRow({ player, isCurrentTurn }: Props) {
  return (
    <div
      className={`
        flex items-center gap-3 px-3 py-2 rounded-lg border transition-all
        ${isCurrentTurn
          ? "border-yellow-500/60 bg-yellow-500/5"
          : "border-stone-700/50 bg-stone-800/40"}
      `}
    >
      <div className={`w-3.5 h-3.5 rounded-full flex-shrink-0 ${COLOR_MAP[player.playerColor]}`} />

      <span className={`flex-1 text-sm tracking-wide ${isCurrentTurn ? "font-bold text-yellow-100" : "font-normal text-stone-300"}`}>
        {player.displayName}
        {isCurrentTurn && <span className="ml-2 text-xs text-yellow-400 font-normal">● TURN</span>}
      </span>

      <div className="flex items-center gap-3 text-xs text-stone-400">
        <span title="Victory Points" className="flex items-center gap-0.5">
          <span className="text-yellow-500">★</span>
          <span className={isCurrentTurn ? "text-yellow-200 font-semibold" : ""}>{player.victoryPoints}</span>
        </span>
        <span title="Resource cards" className="flex items-center gap-0.5">
          <span>🃏</span>{player.resourceCardCount}
        </span>
        <span title="Dev cards" className="flex items-center gap-0.5">
          <span>🎴</span>{player.developmentCardCount}
        </span>
      </div>
    </div>
  );
}
