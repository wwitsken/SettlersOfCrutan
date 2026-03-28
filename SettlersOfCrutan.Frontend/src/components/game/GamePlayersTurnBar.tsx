import type { Game } from "../../domain/game/game";
import type { Player } from "../../domain/game/player";
import type { PlayerColor } from "../../domain/game/gameTypes";

type Props = {
  show: boolean;
  game: Game | null;
  myPlayerId?: string;
};

const PLAYER_COLOR_HEX: Record<PlayerColor, string> = {
  none: "#94a3b8",
  red: "#dc2626",
  blue: "#2563eb",
  white: "#f8fafc",
  orange: "#ea580c",
  green: "#16a34a",
  yellow: "#ca8a04",
  brown: "#78350f",
  purple: "#7c3aed",
};

function PlayerChip({
  player,
  orderLabel,
  isCurrentTurn,
  isMe,
}: {
  player: Player;
  orderLabel: string;
  isCurrentTurn: boolean;
  isMe: boolean;
}) {
  const stroke =
    player.playerColor === "white" ? "border-slate-300" : "border-transparent";
  const handCards =
    player.resourceCardCount + player.developmentCardCount;

  return (
    <li
      className={`flex min-w-0 max-w-[200px] shrink-0 items-center gap-2 rounded-lg border px-2.5 py-1.5 text-left ${
        isCurrentTurn
          ? "border-amber-400 bg-amber-50 shadow-sm"
          : "border-slate-200 bg-slate-50"
      }`}
    >
      <span
        className={`h-3 w-3 shrink-0 rounded-full border ${stroke}`}
        style={{
          backgroundColor: PLAYER_COLOR_HEX[player.playerColor] ?? "#94a3b8",
        }}
        title={player.playerColor}
        aria-hidden
      />
      <div className="min-w-0 flex-1">
        <div className="flex flex-wrap items-baseline gap-x-1.5 gap-y-0">
          <span className="text-[11px] font-semibold tabular-nums text-slate-500">
            {orderLabel}
          </span>
          <span className="truncate text-sm font-medium text-slate-900">
            {player.displayName || "Player"}
          </span>
        </div>
        <div className="mt-0.5 flex flex-wrap gap-1">
          {isMe && (
            <span className="rounded bg-sky-100 px-1 py-0 text-[10px] font-medium text-sky-900">
              You
            </span>
          )}
          {isCurrentTurn && (
            <span className="rounded bg-amber-200/80 px-1 py-0 text-[10px] font-semibold text-amber-950">
              Current turn
            </span>
          )}
        </div>
        <p className="mt-1 text-[10px] tabular-nums text-slate-600">
          {handCards} card{handCards === 1 ? "" : "s"} · {player.victoryPoints}{" "}
          VP
        </p>
        {(player.hasLongestRoad || player.hasLargestArmy) && (
          <div className="mt-0.5 flex flex-wrap gap-1">
            {player.hasLongestRoad && (
              <span className="rounded border border-amber-300/80 bg-amber-50 px-1 py-0 text-[9px] font-semibold text-amber-950">
                Longest road
              </span>
            )}
            {player.hasLargestArmy && (
              <span className="rounded border border-violet-300/80 bg-violet-50 px-1 py-0 text-[9px] font-semibold text-violet-950">
                Largest army
              </span>
            )}
          </div>
        )}
      </div>
    </li>
  );
}

/**
 * Shows all players, turn order, and who is currently playing. DOM only, between
 * the board canvas and the inventory / action bars.
 */
export function GamePlayersTurnBar({ show, game, myPlayerId }: Props) {
  if (!show || !game || game.players.length === 0) return null;

  const ordered = [...game.players].sort(
    (a, b) => a.playOrder - b.playOrder,
  );
  const currentId = game.players[game.playerIndex]?.id;
  const currentPlayer = currentId
    ? ordered.find((p) => p.id === currentId)
    : undefined;

  return (
    <div
      className="shrink-0 border-t border-slate-200 bg-white px-3 py-2"
      role="region"
      aria-label="Players and turn order"
    >
      <div className="mb-1.5 flex flex-wrap items-center justify-between gap-2">
        <div>
          <p className="text-[10px] font-semibold uppercase tracking-wide text-slate-500">
            Players · turn order
          </p>
          <p className="text-[10px] text-slate-400">
            VP from settlements/cities only. Longest road (5+ segments) and
            largest army (3+ knights played).
          </p>
        </div>
        {currentPlayer && (
          <p className="text-xs text-slate-600">
            Now:{" "}
            <span className="font-medium text-slate-900">
              {currentPlayer.displayName || "Player"}
            </span>
            {myPlayerId && currentPlayer.id === myPlayerId ? " (you)" : ""}
          </p>
        )}
      </div>
      <ul className="flex flex-wrap gap-2" aria-label="Players in play order">
        {ordered.map((player, i) => (
          <PlayerChip
            key={player.id}
            player={player}
            orderLabel={`#${i + 1}`}
            isCurrentTurn={player.id === currentId}
            isMe={!!myPlayerId && player.id === myPlayerId}
          />
        ))}
      </ul>
    </div>
  );
}
