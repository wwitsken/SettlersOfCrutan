import type { Player } from "../../types/catan";
import { COLOR_MAP, COLOR_TEXT_MAP } from "../../constants/catanMeta";

export default function PlayerRow({ player }: { player: Player }) {
  return (
    <div
      className={`
        flex items-center gap-3 px-3 py-2 rounded-lg border transition-all
        ${player.isCurrentTurn
          ? "border-yellow-500/60 bg-yellow-500/5"
          : "border-stone-700/50 bg-stone-800/40"}
      `}
    >
      <div className={`w-3.5 h-3.5 rounded-full flex-shrink-0 ${COLOR_MAP[player.color]}`} />

      <span className={`flex-1 text-sm tracking-wide ${player.isCurrentTurn ? "font-bold text-yellow-100" : "font-normal text-stone-300"}`}>
        {player.name}
        {player.isCurrentTurn && <span className="ml-2 text-xs text-yellow-400 font-normal">● TURN</span>}
      </span>

      <div className="flex items-center gap-3 text-xs text-stone-400">
        <span title="Victory Points" className="flex items-center gap-0.5">
          <span className="text-yellow-500">★</span>
          <span className={player.isCurrentTurn ? "text-yellow-200 font-semibold" : ""}>{player.vp}</span>
        </span>
        <span title="Resource cards" className="flex items-center gap-0.5">
          <span>🃏</span>{player.cardCount}
        </span>
        <span title="Dev cards" className="flex items-center gap-0.5">
          <span>🎴</span>{player.devCardCount}
        </span>
      </div>
    </div>
  );
}
