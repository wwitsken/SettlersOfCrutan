import type { Player } from "../../domain/game/player";
import CatanAvatar from "../ui/CatanAvatar";

type Props = {
  player: Player;
  isCurrentTurn: boolean;
};

export default function PlayerRow({ player, isCurrentTurn }: Props) {
  return (
    <div
      className="flex items-center gap-2.5 rounded-xl border-2 px-3 py-2 shadow-[2px_2px_0_var(--ink)] transition-all"
      style={{
        borderColor: isCurrentTurn ? "var(--catan-accent)" : "var(--ink)",
        background: isCurrentTurn ? "rgba(178,58,42,0.07)" : "var(--parchment-2)",
        outline: isCurrentTurn ? "3px dashed var(--catan-accent)" : undefined,
        outlineOffset: isCurrentTurn ? "2px" : undefined,
      }}
    >
      <CatanAvatar color={player.playerColor} name={player.displayName} size="sm" />

      <div className="flex-1 min-w-0 overflow-hidden">
        <div
          className="truncate"
          style={{
            fontFamily: "var(--font-serif)",
            fontSize: "0.95rem",
            color: "var(--ink)",
          }}
        >
          {player.displayName}
        </div>
        {isCurrentTurn && (
          <div
            style={{
              fontFamily: "var(--font-mono)",
              fontSize: "0.65rem",
              letterSpacing: "0.1em",
              color: "var(--catan-accent)",
            }}
          >
            THEIR TURN
          </div>
        )}
      </div>

      <div
        className="flex items-center gap-2 shrink-0"
        style={{ fontFamily: "var(--font-hand)", color: "var(--ink-faint)", fontSize: "0.9rem" }}
      >
        <span title="Victory Points" style={{ fontFamily: "var(--font-serif)", fontSize: "1.1rem", color: "var(--ink)" }}>
          {player.victoryPoints}
          <span style={{ fontSize: "0.7rem", color: "var(--ink-faint)", marginLeft: 2 }}>VP</span>
        </span>
        <span title="Resource cards">🃏{player.resourceCardCount}</span>
        <span title="Dev cards">🎴{player.developmentCardCount}</span>
      </div>
    </div>
  );
}
