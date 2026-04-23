import type { Game } from "../../domain/game/game";
import CatanButton from "../ui/CatanButton";

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

/**
 * Vertical action tent rendered in the left column of CatanLayout.
 * Not inside the R3F Canvas.
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
  if (!show || !game || !gameId) {
    return (
      <div
        style={{
          color: "var(--ink-faint)",
          fontFamily: "var(--font-hand)",
          fontSize: "1rem",
        }}
      >
        Waiting for game…
      </div>
    );
  }

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
      className="flex flex-col gap-2"
      role="toolbar"
      aria-label="Game actions"
    >
      {/* Phase / status hint */}
      <div
        className="text-xs tracking-widest"
        style={{ fontFamily: "var(--font-mono)", color: "var(--ink-faint)" }}
      >
        {devRoadPicking
          ? "PLACE ROADS"
          : awaitingKnightRobberHex || (isMyTurn && phase === "resolveRobber")
            ? "MOVE ROBBER"
            : isMyTurn
              ? phase.toUpperCase()
              : "WAITING…"}
      </div>

      {actionError && (
        <div
          className="rounded-xl border-2 border-(--catan-accent) bg-red-50 px-2 py-1.5 text-xs"
          style={{ color: "var(--catan-accent)" }}
          role="alert"
        >
          <span>{actionError}</span>{" "}
          <button
            type="button"
            className="underline cursor-pointer"
            onClick={onClearActionError}
            style={{ color: "var(--catan-accent)" }}
          >
            Dismiss
          </button>
        </div>
      )}

      <div
        className="my-0.5 h-0.5"
        style={{
          background:
            "repeating-linear-gradient(90deg, var(--ink-soft) 0 6px, transparent 6px 10px)",
        }}
      />

      <CatanButton
        variant="primary"
        className="w-full justify-center"
        disabled={!canRoll}
        onClick={onRollDice}
      >
        🎲 Roll Dice
      </CatanButton>

      <CatanButton
        variant="ghost"
        className="w-full justify-center"
        disabled={!canEndTurn}
        onClick={onEndTurn}
      >
        End Turn ▸
      </CatanButton>

      <CatanButton
        className="w-full justify-center"
        disabled={!canProposeTrade}
        onClick={onProposeTrade}
      >
        🤝 Offer Trade
      </CatanButton>

      <CatanButton
        className="w-full justify-center"
        disabled={!canBuyDev}
        onClick={onBuyDevCard}
      >
        📜 Buy Dev Card
      </CatanButton>
    </div>
  );
}
