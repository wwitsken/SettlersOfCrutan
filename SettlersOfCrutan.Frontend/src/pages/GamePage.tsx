import { useEffect } from "react";
import { useParams } from "react-router";
import { CatanBoardScene } from "../components/board/CatanBoardScene";
import { DevDebugPanel } from "../components/dev/DevDebugPanel";
import { GameStoreDebugView } from "../components/dev/GameStoreDebugView";
import { useGameSignalR } from "../hooks/useGameSignalR";
import { useGamesStore } from "../stores/gameStore";
import { api } from "../api/client";
import { acquireAccessToken } from "../authConfig";
import { resolveBoardView } from "../domain/game/boardView";
import { game as exampleGame } from "../domain/game/gameExample";
import { applyGamePayloadFromApi } from "../stores/applyGamePayload";

function GamePage() {
  const { gameId } = useParams();
  const game = useGamesStore((s) => s.game);
  const setLoading = useGamesStore((s) => s.setLoading);
  const setError = useGamesStore((s) => s.setError);
  const status = useGamesStore((s) => s.status);
  const loadError = useGamesStore((s) => s.error);

  const { isConnected, isConnecting, error: hubError } = useGameSignalR(
    gameId ?? null,
  );

  useEffect(() => {
    if (!gameId) return;

    let cancelled = false;
    (async () => {
      setLoading(gameId);
      const { data, error, response } = await api.GET("/api/games/{id}", {
        params: { path: { id: gameId } },
        accessToken: await acquireAccessToken(),
      });
      if (cancelled) return;
      if (error || response.status !== 200 || !data) {
        setError(
          error
            ? "Could not load game."
            : `Game request failed (${response.status}).`,
        );
        return;
      }
      if (!applyGamePayloadFromApi(data))
        setError("Game response could not be mapped.");
    })();

    return () => {
      cancelled = true;
    };
  }, [gameId, setLoading, setError]);

  const boardView = resolveBoardView(status, game);
  const boardGame = boardView === "loading" ? exampleGame : game ?? exampleGame;
  const showExampleBanner = boardView === "example";
  const showLoadingBanner = boardView === "loading";

  return (
    <div className="flex flex-col gap-3">
      <div className="flex flex-wrap items-center justify-between gap-2 rounded-xl border border-slate-200 bg-white px-4 py-3 shadow-sm">
        <div>
          <h1 className="text-lg font-semibold text-slate-900">
            {game?.gameName ?? "Game"}
          </h1>
          <p className="text-xs text-slate-500">
            {gameId ? `Id ${gameId}` : "No game id"}{" "}
            {game && (
              <>
                · Phase <span className="font-medium">{game.gamePhase}</span> ·
                Round {game.round}
              </>
            )}
          </p>
        </div>
        <div className="text-right text-xs text-slate-600">
          {isConnecting && !isConnected && <span>Connecting to hub…</span>}
          {hubError && !isConnected && (
            <span className="text-red-600">Hub: {hubError}</span>
          )}
          {isConnected && <span className="text-emerald-600">Live updates</span>}
        </div>
      </div>

      {loadError && (
        <div
          className="rounded-lg border border-red-200 bg-red-50 px-3 py-2 text-sm text-red-800"
          role="alert"
        >
          {loadError}
          {showExampleBanner && (
            <span className="mt-1 block text-xs text-red-700">
              Showing static example board until the server returns a game.
            </span>
          )}
        </div>
      )}

      <div className="flex flex-col gap-1">
        {(showLoadingBanner || showExampleBanner) && (
          <div
            className={`rounded-lg border px-3 py-1.5 text-center text-xs font-medium ${
              showLoadingBanner
                ? "border-sky-200 bg-sky-50 text-sky-900"
                : "border-amber-200 bg-amber-50 text-amber-950"
            }`}
            role="status"
          >
            {showLoadingBanner
              ? "Loading game… (board preview is static example data)"
              : "Example board — store has no loaded game yet (see debug panel below in dev)."}
          </div>
        )}
        <div
          className="overflow-hidden rounded-xl border border-slate-200 bg-slate-950 shadow-inner"
          style={{ height: "min(70vh, 640px)", minHeight: 360 }}
        >
          <CatanBoardScene game={boardGame} hexRadius={1} />
        </div>
      </div>

      {import.meta.env.DEV && <GameStoreDebugView />}

      <DevDebugPanel gameId={gameId} />
    </div>
  );
}

export default GamePage;
