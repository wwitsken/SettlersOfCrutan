import { useCallback, useState } from "react";
import { api } from "../../api/client";

type Props = {
  gameId: string | undefined;
};

/**
 * Dev-only shortcuts that hit the same HTTP API as the UI. Stripped from production builds.
 */
export function DevDebugPanel({ gameId }: Props) {
  const [log, setLog] = useState<string>("");

  const append = useCallback((line: string) => {
    setLog((prev) => `${line}\n${prev}`.slice(0, 4000));
  }, []);

  if (!import.meta.env.DEV) return null;

  return (
    <div className="fixed bottom-4 right-4 z-50 max-w-sm rounded-lg border border-amber-700/50 bg-stone-900/95 p-3 text-left text-xs text-amber-100 shadow-xl">
      <div className="mb-2 font-semibold text-amber-400">Dev debug</div>
      {!gameId && (
        <p className="text-stone-400">Open a game route to enable actions.</p>
      )}
      {gameId && (
        <div className="flex flex-wrap gap-2">
          <button
            type="button"
            className="rounded border border-stone-600 px-2 py-1 hover:bg-stone-800"
            onClick={async () => {
              const { data, error } = await api.POST(
                "/api/games/{id}/play/roll-dice",
                { params: { path: { id: gameId } } },
              );
              append(
                error
                  ? `roll-dice error: ${JSON.stringify(error)}`
                  : `roll-dice: ${JSON.stringify(data)}`,
              );
            }}
          >
            Roll dice
          </button>
          <button
            type="button"
            className="rounded border border-stone-600 px-2 py-1 hover:bg-stone-800"
            onClick={async () => {
              const { data, error } = await api.POST("/api/games/{id}/turn/end", {
                params: { path: { id: gameId } },
              });
              append(
                error
                  ? `end-turn error: ${JSON.stringify(error)}`
                  : `end-turn: ${JSON.stringify(data)}`,
              );
            }}
          >
            End turn
          </button>
          <button
            type="button"
            className="rounded border border-stone-600 px-2 py-1 hover:bg-stone-800"
            onClick={async () => {
              const { data, error } = await api.GET("/api/games/{id}", {
                params: { path: { id: gameId } },
              });
              append(
                error
                  ? `GET game error: ${JSON.stringify(error)}`
                  : `GET game ok (phase ${data?.gamePhase})`,
              );
            }}
          >
            Refresh GET
          </button>
        </div>
      )}
      {log ? (
        <pre className="mt-2 max-h-32 overflow-auto whitespace-pre-wrap text-[10px] text-stone-300">
          {log}
        </pre>
      ) : null}
    </div>
  );
}
