import type { Game } from "../../domain/game/game";
import type { GamePageInteractionMode } from "../../hooks/useGamePageInteraction";
import type { MaritimeRatio } from "./MaritimeTradeDialog";

type Props = {
  show: boolean;
  game: Game | null;
  gameId: string | undefined;
  isMyTurn: boolean;
  hasPrivateSlice: boolean;
  interactionMode: GamePageInteractionMode;
  onInteractionMode: (m: GamePageInteractionMode) => void;
  actionError: string | null;
  onClearActionError: () => void;
  onRollDice: () => void;
  onEndTurn: () => void;
  onBuyDevCard: () => void;
  /** Dev-card plays (opens pickers or board modes from parent) */
  onPlayKnight: () => void;
  onPlayMonopoly: () => void;
  onPlayYearOfPlenty: () => void;
  onPlayRoadBuilding: () => void;
  onStartRobber: () => void;
  onProposeTrade: () => void;
  onMaritimeTrade: (ratio: MaritimeRatio) => void;
};

const btn =
  "rounded-md border px-2 py-1 text-xs font-medium transition disabled:cursor-not-allowed disabled:opacity-40";
const btnPrimary = `${btn} border-stone-600 bg-stone-800/60 text-stone-200 hover:bg-stone-700/60`;
const btnActive = `${btn} border-sky-500 bg-sky-900/40 text-sky-300`;
const btnDanger = `${btn} border-red-700/60 bg-red-900/30 text-red-400 hover:bg-red-900/50`;

/**
 * DOM toolbar under the R3F canvas — never inside `<Canvas>`.
 */
export function GameActionBar({
  show,
  game,
  gameId,
  isMyTurn,
  hasPrivateSlice,
  interactionMode,
  onInteractionMode,
  actionError,
  onClearActionError,
  onRollDice,
  onEndTurn,
  onBuyDevCard,
  onPlayKnight,
  onPlayMonopoly,
  onPlayYearOfPlenty,
  onPlayRoadBuilding,
  onStartRobber,
  onProposeTrade,
  onMaritimeTrade,
}: Props) {
  if (!show || !game || !gameId) return null;

  const phase = game.gamePhase;
  const idle = interactionMode === "idle";

  const canRoll = isMyTurn && phase === "rollDice" && idle;
  const canEndTurn =
    isMyTurn && (phase === "tradeBuild" || phase === "setup") && idle;
  const canBuild =
    isMyTurn && phase === "tradeBuild" && idle && hasPrivateSlice;
  const canBuyDev = canBuild;
  const canDevPlays = canBuild;
  const canProposeTrade = canBuild && !game.currentTradeOffer;
  const canRobber =
    isMyTurn && phase === "resolveRobber" && idle && hasPrivateSlice;
  const setupPlacing =
    isMyTurn && phase === "setup" && idle && hasPrivateSlice;

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
        {interactionMode !== "idle" && (
          <span className="text-xs font-medium text-sky-400">
            Mode: {interactionMode}
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

        <button
          type="button"
          className={btnPrimary}
          disabled={!canBuild}
          onClick={() => onMaritimeTrade(4)}
        >
          Maritime 4:1
        </button>
        <button
          type="button"
          className={btnPrimary}
          disabled={!canBuild}
          onClick={() => onMaritimeTrade(3)}
        >
          Maritime 3:1
        </button>
        <button
          type="button"
          className={btnPrimary}
          disabled={!canBuild}
          onClick={() => onMaritimeTrade(2)}
        >
          Maritime 2:1
        </button>

        <button
          type="button"
          className={btnPrimary}
          disabled={!canRobber}
          onClick={onStartRobber}
        >
          Move robber
        </button>

        <span className="mx-1 hidden h-6 w-px bg-stone-700 sm:inline" />

        <button
          type="button"
          className={
            interactionMode === "buildRoad" ? btnActive : btnPrimary
          }
          disabled={!canBuild}
          onClick={() =>
            onInteractionMode(
              interactionMode === "buildRoad" ? "idle" : "buildRoad",
            )
          }
        >
          Build road
        </button>
        <button
          type="button"
          className={
            interactionMode === "buildSettlement" ? btnActive : btnPrimary
          }
          disabled={!canBuild}
          onClick={() =>
            onInteractionMode(
              interactionMode === "buildSettlement"
                ? "idle"
                : "buildSettlement",
            )
          }
        >
          Build settlement
        </button>
        <button
          type="button"
          className={
            interactionMode === "upgradeCity" ? btnActive : btnPrimary
          }
          disabled={!canBuild}
          onClick={() =>
            onInteractionMode(
              interactionMode === "upgradeCity" ? "idle" : "upgradeCity",
            )
          }
        >
          Upgrade to city
        </button>

        <span className="mx-1 hidden h-6 w-px bg-stone-700 sm:inline" />

        <button
          type="button"
          className={btnPrimary}
          disabled={!setupPlacing}
          onClick={() =>
            onInteractionMode(
              interactionMode === "initialSettle" ? "idle" : "initialSettle",
            )
          }
        >
          {interactionMode === "initialSettle"
            ? "Cancel settlement pick"
            : "Place initial settlement"}
        </button>

        <span className="mx-1 hidden h-6 w-px bg-stone-700 sm:inline" />

        <button
          type="button"
          className={btnPrimary}
          disabled={!canDevPlays}
          onClick={onPlayKnight}
        >
          Play knight
        </button>
        <button
          type="button"
          className={btnPrimary}
          disabled={!canDevPlays}
          onClick={onPlayMonopoly}
        >
          Monopoly
        </button>
        <button
          type="button"
          className={btnPrimary}
          disabled={!canDevPlays}
          onClick={onPlayYearOfPlenty}
        >
          Year of plenty
        </button>
        <button
          type="button"
          className={btnPrimary}
          disabled={!canDevPlays}
          onClick={onPlayRoadBuilding}
        >
          Road building (2 roads)
        </button>

        {interactionMode !== "idle" && (
          <button
            type="button"
            className={btnDanger}
            onClick={() => onInteractionMode("idle")}
          >
            Cancel board action
          </button>
        )}
      </div>
    </div>
  );
}
