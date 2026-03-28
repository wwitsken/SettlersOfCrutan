import { useMemo } from "react";
import { useGamesStore } from "../../stores/gameStore";
import { game as exampleGame } from "../../domain/game/gameExample";
import { resolveBoardView } from "../../domain/game/boardView";
import { getCurrentPlayer } from "../../domain/game/selectors";

function serializeForDebug(value: unknown): string {
  return JSON.stringify(
    value,
    (_key, v) => (v instanceof Date ? v.toISOString() : v),
    2,
  );
}

/**
 * In-flow panel under the board: current `useGamesStore` game slice.
 * Mount only when `import.meta.env.DEV` (see GamePage).
 */
export function GameStoreDebugView() {
  const status = useGamesStore((s) => s.status);
  const error = useGamesStore((s) => s.error);
  const currentGameId = useGamesStore((s) => s.currentGameId);
  const game = useGamesStore((s) => s.game);
  const privateGame = useGamesStore((s) => s.privateGame);

  const boardView = resolveBoardView(status, game);
  const me = game ? getCurrentPlayer(game, privateGame) : undefined;

  const payload = useMemo(
    () => ({
      status,
      error,
      currentGameId,
      boardView,
      currentPlayerFromPrivateSlice: me?.displayName ?? me?.id ?? null,
      game,
      privateGame,
    }),
    [status, error, currentGameId, boardView, me, game, privateGame],
  );

  const json = useMemo(() => serializeForDebug(payload), [payload]);

  const boardSourceLabel =
    boardView === "loading"
      ? "loading (canvas uses exampleGame)"
      : boardView === "live"
        ? "live (store `game`)"
        : "example (canvas uses exampleGame; store `game` null)";

  return (
    <div className="rounded-xl border border-dashed border-slate-400/80 bg-slate-100/90 px-3 py-2 text-left shadow-inner dark:border-slate-600 dark:bg-slate-900/80">
      <div className="mb-1 flex flex-wrap items-baseline justify-between gap-2 border-b border-slate-300/80 pb-1 dark:border-slate-600">
        <span className="text-xs font-semibold tracking-wide text-slate-700 dark:text-slate-200">
          Zustand · useGamesStore
        </span>
        <span className="text-[10px] text-slate-500 dark:text-slate-400">
          Board: {boardSourceLabel}
        </span>
      </div>
      {game && !privateGame && (
        <p className="mb-1 text-[10px] text-slate-600 dark:text-slate-400">
          <span className="font-medium">Private slice:</span>{" "}
          <code className="rounded bg-slate-200/80 px-0.5 dark:bg-slate-800">
            privateGame
          </code>{" "}
          is null until a player-scoped payload (GET GameDto / SignalR) provides
          it.
        </p>
      )}
      {!game && (
        <p className="mb-1 text-[10px] text-amber-800 dark:text-amber-200/90">
          <span className="font-medium">Note:</span>{" "}
          <code className="rounded bg-slate-200/80 px-0.5 dark:bg-slate-800">
            game
          </code>{" "}
          is null — 3D board uses{" "}
          <code className="rounded bg-slate-200/80 px-0.5 dark:bg-slate-800">
            exampleGame
          </code>{" "}
          (id &quot;{exampleGame.id}&quot;).
        </p>
      )}
      <pre className="max-h-72 overflow-auto text-[11px] leading-snug text-slate-800 dark:text-slate-200">
        {json}
      </pre>
    </div>
  );
}
