import type { Game } from "../../domain/game/game";

type Props = {
  show: boolean;
  game: Game | null;
  gameId: string | undefined;
  isMyTurn: boolean;
  hasPrivateSlice: boolean;
  /** After initial settlement vertex chosen, road must be placed before ending turn. */
  setupAwaitingInitialRoad: boolean;
  devRoadPicking: boolean;
  awaitingKnightRobberHex: boolean;
  actionError: string | null;
  onClearActionError: () => void;
  onRollDice: () => void;
  onEndTurn: () => void;
  onBuyDevCard: () => void;
  onProposeTrade: () => void;
};

const btn =
  "rounded-md border px-2 py-1 text-xs font-medium transition disabled:cursor-not-allowed disabled:opacity-40";
const btnPrimary = `${btn} border-stone-600 bg-stone-800/60 text-stone-200 hover:bg-stone-700/60`;

/**
 * DOM toolbar under the R3F canvas — never inside `<Canvas>`.
 */
export function GameActionBar({
  show,
  game,
  gameId,
  isMyTurn,
  hasPrivateSlice,
  setupAwaitingInitialRoad,
  devRoadPicking,
  awaitingKnightRobberHex,
  actionError,
  onClearActionError,
  onRollDice,
  onEndTurn,
  onBuyDevCard,
  onProposeTrade,
}: Props) {
  if (!show || !game || !gameId) return null;

  const phase = game.gamePhase;

  const blockForRobberFlow =
    (isMyTurn && phase === "resolveRobber") || awaitingKnightRobberHex;
  const toolbarIdle = !devRoadPicking && !blockForRobberFlow;

  const canRoll = isMyTurn && phase === "rollDice" && toolbarIdle;
  const canEndTurn =
    isMyTurn &&
    (phase === "tradeBuild" || phase === "setup") &&
    toolbarIdle &&
    !setupAwaitingInitialRoad;
  const canBuild =
    isMyTurn && phase === "tradeBuild" && toolbarIdle && hasPrivateSlice;
  const canBuyDev = canBuild;
  const canProposeTrade = canBuild && !game.currentTradeOffer;

  return (
    <div
      className="shrink-0 border-t border-stone-700/60 bg-transparent px-3 py-2"
      role="toolbar"
      aria-label="Game actions"
    >
      {actionError && (
        <div className="mb-2 flex items-start justify-between gap-2 rounded border border-red-700/40 bg-red-900/20 px-2 py-1 text-xs text-red-300">
          <span>{actionError}</span>
          <button
            type="button"
            className="shrink-0 text-red-400 underline"
            onClick={onClearActionError}
          >
            Dismiss
          </button>
        </div>
      )}

      <div className="mb-1.5 flex flex-wrap items-center gap-2">
        <span className="text-xs font-medium text-stone-400">
          Phase <span className="text-stone-200">{phase}</span>
        </span>
        {!isMyTurn && (
          <span className="text-xs text-stone-500">Waiting for other player…</span>
        )}
        {devRoadPicking && (
          <span className="text-xs font-medium text-sky-400">
            Place two roads (road building card)
          </span>
        )}
        {awaitingKnightRobberHex && (
          <span className="text-xs font-medium text-sky-400">
            Move the robber (knight)
          </span>
        )}
        {isMyTurn && phase === "resolveRobber" && (
          <span className="text-xs font-medium text-sky-400">
            Move the robber
          </span>
        )}
      </div>

      <div className="flex flex-wrap gap-1.5">
        <button
          type="button"
          className={btnPrimary}
          disabled={!canRoll}
          onClick={onRollDice}
        >
          Roll dice
        </button>

        <button
          type="button"
          className={btnPrimary}
          disabled={!canEndTurn}
          onClick={onEndTurn}
        >
          End turn
        </button>

        <button
          type="button"
          className={btnPrimary}
          disabled={!canBuyDev}
          onClick={onBuyDevCard}
        >
          Buy dev card
        </button>

        <button
          type="button"
          className={btnPrimary}
          disabled={!canProposeTrade}
          onClick={onProposeTrade}
        >
          Propose trade
        </button>
      </div>
    </div>
  );
}
