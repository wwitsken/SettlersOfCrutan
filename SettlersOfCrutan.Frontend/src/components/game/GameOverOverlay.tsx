import { useMemo, useState } from "react";
import { useNavigate } from "react-router";
import type { Game } from "../../domain/game/game";
import type { PlayerColor } from "../../domain/game/gameTypes";
import type {
  FinalPlayerScorePayload,
  GameEndedPayload,
} from "../../api/realtimeEvents";
import { playerColorToHex } from "../../domain/game/playerColorHex";
import ParchmentCard from "../ui/ParchmentCard";
import CatanButton from "../ui/CatanButton";

type Props = {
  /** Rich ceremony payload from the `GameEnded` SignalR event. Null until received. */
  payload: GameEndedPayload | null;
  /** Current game snapshot (used for fallback scoreboard on reload / late-join). */
  game: Game | null;
};

type Row = {
  key: string;
  displayName: string;
  color: PlayerColor;
  victoryPoints: number;
  observableVictoryPoints: number;
  hiddenVictoryPointCards: number;
  hasLongestRoad: boolean;
  hasLargestArmy: boolean;
  isWinner: boolean;
};

/**
 * Full-screen celebratory overlay shown once a player reaches the win threshold.
 * Renders when either:
 *  - a rich `GameEnded` payload is present (preferred — reveals hidden VP cards), or
 *  - `game.gamePhase === "gameEnd"` (fallback — public VP only).
 * Dismissible locally via Close; the game itself remains in `gameEnd` on the server.
 */
export default function GameOverOverlay({ payload, game }: Props) {
  const navigate = useNavigate();
  const [dismissed, setDismissed] = useState(false);

  const rows = useMemo<Row[]>(() => buildRows(payload, game), [payload, game]);
  const winner = useMemo(() => {
    if (rows.length === 0) return null;
    return rows.find((r) => r.isWinner) ?? rows[0];
  }, [rows]);

  const isGameEnd =
    payload !== null || (game?.gamePhase === "gameEnd" && rows.length > 0);
  if (!isGameEnd || dismissed || winner === null) return null;

  return (
    <div
      className="fixed inset-0 z-[70] flex items-center justify-center bg-black/50 p-4"
      role="dialog"
      aria-modal="true"
      aria-labelledby="game-over-title"
    >
      <ParchmentCard
        tape
        padding="p-6"
        className="w-full max-w-lg max-h-[90vh] overflow-auto font-(--font-hand)"
      >
        <header className="text-center">
          <p className="text-sm uppercase tracking-[0.3em] text-(--ink-faint)">
            Game Over
          </p>
          <h2
            id="game-over-title"
            className="mt-1 text-4xl font-bold text-(--ink)"
          >
            Victory!
          </h2>
          <div className="mt-3 flex items-center justify-center gap-3 text-xl text-(--ink)">
            <ColorChip color={winner.color} size={18} />
            <span className="font-bold">{winner.displayName || "Winner"}</span>
            <span className="text-(--ink-soft)">—</span>
            <span className="tabular-nums">
              {winner.victoryPoints} VP
            </span>
          </div>
        </header>

        <section className="mt-5 border-t-2 border-(--ink) pt-4">
          <h3 className="mb-2 text-base uppercase tracking-wider text-(--ink-soft)">
            Final Scoreboard
          </h3>
          <ul className="divide-y divide-(--ink-faint)/30">
            {rows.map((r) => (
              <li
                key={r.key}
                className={`flex items-center justify-between gap-3 py-2 text-(--ink) ${
                  r.isWinner ? "font-bold" : ""
                }`}
              >
                <div className="flex min-w-0 items-center gap-2">
                  <ColorChip color={r.color} size={14} />
                  <span className="truncate">{r.displayName || "—"}</span>
                  {r.hasLongestRoad && (
                    <Badge title="Longest road">LR</Badge>
                  )}
                  {r.hasLargestArmy && (
                    <Badge title="Largest army">LA</Badge>
                  )}
                </div>
                <div className="flex items-center gap-4 tabular-nums text-sm text-(--ink-soft)">
                  <span title="Observable VP">
                    {r.observableVictoryPoints}
                  </span>
                  {r.hiddenVictoryPointCards > 0 && (
                    <span title="Hidden VP development cards">
                      +{r.hiddenVictoryPointCards}
                    </span>
                  )}
                  <span className="text-lg text-(--ink)">
                    {r.victoryPoints}
                  </span>
                </div>
              </li>
            ))}
          </ul>
          {payload === null && (
            <p className="mt-3 text-xs text-(--ink-faint)">
              Hidden victory-point cards are only shown in the live end-game
              ceremony.
            </p>
          )}
        </section>

        <footer className="mt-6 flex items-center justify-end gap-3">
          <CatanButton
            variant="ghost"
            size="sm"
            onClick={() => setDismissed(true)}
          >
            Close
          </CatanButton>
          <CatanButton
            variant="primary"
            size="sm"
            onClick={() => navigate("/")}
          >
            Back to Home
          </CatanButton>
        </footer>
      </ParchmentCard>
    </div>
  );
}

function ColorChip({ color, size }: { color: PlayerColor; size: number }) {
  return (
    <span
      aria-hidden
      className="inline-block rounded-full border border-(--ink)"
      style={{
        width: size,
        height: size,
        backgroundColor: playerColorToHex(color),
      }}
    />
  );
}

function Badge({
  children,
  title,
}: {
  children: React.ReactNode;
  title: string;
}) {
  return (
    <span
      title={title}
      className="ml-1 rounded border border-(--ink) bg-(--parchment-2) px-1 text-[10px] font-semibold uppercase tracking-wider text-(--ink)"
    >
      {children}
    </span>
  );
}

function buildRows(
  payload: GameEndedPayload | null,
  game: Game | null,
): Row[] {
  if (payload !== null) {
    return [...payload.finalScores]
      .sort((a, b) => b.victoryPoints - a.victoryPoints)
      .map((s) => payloadToRow(s, payload.winnerPlayerId));
  }

  if (game?.gamePhase !== "gameEnd" || !game.players?.length) return [];

  const winnerId = game.winnerPlayerId;
  return [...game.players]
    .sort((a, b) => b.victoryPoints - a.victoryPoints)
    .map((p) => ({
      key: p.id,
      displayName: p.displayName,
      color: p.playerColor,
      victoryPoints: p.victoryPoints,
      observableVictoryPoints: p.victoryPoints,
      hiddenVictoryPointCards: 0,
      hasLongestRoad: p.hasLongestRoad,
      hasLargestArmy: p.hasLargestArmy,
      isWinner: winnerId != null ? p.id === winnerId : false,
    }));
}

function payloadToRow(s: FinalPlayerScorePayload, winnerId: string): Row {
  return {
    key: s.playerId,
    displayName: s.displayName,
    color: s.playerColor,
    victoryPoints: s.victoryPoints,
    observableVictoryPoints: s.observableVictoryPoints,
    hiddenVictoryPointCards: s.hiddenVictoryPointCards,
    hasLongestRoad: s.hasLongestRoad,
    hasLargestArmy: s.hasLargestArmy,
    isWinner: s.playerId === winnerId,
  };
}
