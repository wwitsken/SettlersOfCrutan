import { useCallback, useEffect, useMemo, useState } from "react";
import { useParams } from "react-router";
import { CatanBoardScene } from "../components/board/CatanBoardScene";
import { GameStoreDebugView } from "../components/dev/GameStoreDebugView";
import { useGameSignalR } from "../hooks/useGameSignalR";
import { useGamesStore } from "../stores/gameStore";
import { api } from "../api/client";
import { acquireAccessToken } from "../authConfig";
import { resolveBoardView } from "../domain/game/boardView";
import { game as exampleGame } from "../domain/game/gameExample";
import { applyGamePayloadFromApi } from "../stores/applyGamePayload";
import { GameActionBar } from "../components/game/GameActionBar";
import { GamePlayersTurnBar } from "../components/game/GamePlayersTurnBar";
import { PlayerInventoryStrip } from "../components/game/PlayerInventoryStrip";
import { DiscardHalfDialog } from "../components/game/DiscardHalfDialog";
import { RobberVictimPicker } from "../components/game/RobberVictimPicker";
import { DevCardResourceDialog } from "../components/game/DevCardResourceDialog";
import {
  boardPickModeFromInteraction,
  useGamePageInteraction,
} from "../hooks/useGamePageInteraction";
import { getCurrentPlayer } from "../domain/game/selectors";
import { getPlayerIdsExposedToHex } from "../domain/game/robberExposure";
import {
  toHexCoordDto,
  vertexDtoToHexes,
} from "../domain/game/hexCoords";
import type { HexCoordinate } from "../domain/game/board";
import type { components } from "../api/types";
import {
  postBuildRoad,
  postBuildSettlement,
  postBuyDevelopmentCard,
  postEndTurn,
  postPlaceInitial,
  postResolveRobber,
  postRollDice,
  postUpgradeCity,
  postUseKnight,
  postUseMonopoly,
  postUseRoadBuilding,
  postUseYearOfPlenty,
} from "../api/gameCommands";

type EdgeCoordDto = components["schemas"]["EdgeCoordDto"];
type VertexCoordDto = components["schemas"]["VertexCoordDto"];

function GamePage() {
  const { gameId } = useParams();
  const game = useGamesStore((s) => s.game);
  const privateGame = useGamesStore((s) => s.privateGame);
  const setLoading = useGamesStore((s) => s.setLoading);
  const setError = useGamesStore((s) => s.setError);
  const status = useGamesStore((s) => s.status);
  const loadError = useGamesStore((s) => s.error);

  const {
    interactionMode,
    setInteractionMode,
    pendingInitialVertex,
    setPendingInitialVertex,
    pendingRobberHex,
    setPendingRobberHex,
    robberKind,
    setRobberKind,
    devRoadFirstEdge,
    setDevRoadFirstEdge,
    actionError,
    setActionError,
    clearRobberPending,
    resetDevRoad,
  } = useGamePageInteraction();

  const [monopolyOpen, setMonopolyOpen] = useState(false);
  const [yearOpen, setYearOpen] = useState(false);
  const [devBusy, setDevBusy] = useState(false);
  const [devErr, setDevErr] = useState<string | null>(null);
  const [robberBusy, setRobberBusy] = useState(false);
  const [robberErr, setRobberErr] = useState<string | null>(null);

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

  const me = game && privateGame ? getCurrentPlayer(game, privateGame) : undefined;
  const currentPid =
    game && game.players.length > 0
      ? game.players[game.playerIndex]?.id
      : undefined;
  const isMyTurn = !!(
    privateGame &&
    currentPid &&
    currentPid === privateGame.myPlayerId
  );

  const boardPickMode =
    boardView === "live"
      ? boardPickModeFromInteraction(interactionMode)
      : "none";

  const initialRoadHexes = useMemo(
    () =>
      pendingInitialVertex ? vertexDtoToHexes(pendingInitialVertex) : null,
    [pendingInitialVertex],
  );

  const showLiveHud = boardView === "live" && !!game && !!gameId;

  const mustDiscard =
    showLiveHud &&
    game!.gamePhase === "discardHalf" &&
    me &&
    me.discardRequirement > 0;

  const victimChoices = useMemo(() => {
    if (!game || !privateGame || !pendingRobberHex) return [];
    return getPlayerIdsExposedToHex(game, pendingRobberHex)
      .filter((id) => id !== privateGame.myPlayerId)
      .map((id) => ({
        id,
        displayName: game.players.find((p) => p.id === id)?.displayName ?? id,
      }));
  }, [game, privateGame, pendingRobberHex]);

  const runCmd = useCallback(
    async (fn: () => Promise<{ ok: boolean; errorMessage?: string }>) => {
      setActionError(null);
      const r = await fn();
      if (!r.ok) setActionError(r.errorMessage ?? "Action failed.");
      return r;
    },
    [setActionError],
  );

  const handleRollDice = () => {
    if (!gameId) return;
    void runCmd(() => postRollDice(gameId));
  };

  const handleEndTurn = () => {
    if (!gameId) return;
    void runCmd(() => postEndTurn(gameId));
  };

  const handleBuyDevCard = () => {
    if (!gameId) return;
    void runCmd(() => postBuyDevelopmentCard(gameId));
  };

  const handleStartRobber = () => {
    setRobberKind("resolve");
    setInteractionMode("robberHex");
  };

  const handleRobberHexChosen = useCallback(
    (hex: HexCoordinate) => {
      if (!game || !privateGame || !gameId || !robberKind) return;

      const victims = getPlayerIdsExposedToHex(game, hex).filter(
        (id) => id !== privateGame.myPlayerId,
      );

      const finish = async (victimId: string) => {
        setRobberBusy(true);
        setRobberErr(null);
        const hexDto = toHexCoordDto(hex);
        const r =
          robberKind === "resolve"
            ? await postResolveRobber(gameId, victimId, hexDto)
            : await postUseKnight(
                gameId,
                privateGame.myPlayerId,
                victimId,
                hexDto,
              );
        setRobberBusy(false);
        if (r.ok) {
          clearRobberPending();
          setInteractionMode("idle");
        } else {
          setRobberErr(r.errorMessage);
        }
      };

      if (victims.length === 1) {
        void finish(victims[0]!);
        return;
      }

      setPendingRobberHex(hex);
      setInteractionMode("idle");
    },
    [
      game,
      privateGame,
      gameId,
      robberKind,
      clearRobberPending,
      setInteractionMode,
      setPendingRobberHex,
    ],
  );

  const handleVictimPick = (victimId: string) => {
    if (!pendingRobberHex || !gameId || !privateGame || !robberKind) return;
    void (async () => {
      setRobberBusy(true);
      setRobberErr(null);
      const hexDto = toHexCoordDto(pendingRobberHex);
      const r =
        robberKind === "resolve"
          ? await postResolveRobber(gameId, victimId, hexDto)
          : await postUseKnight(
              gameId,
              privateGame.myPlayerId,
              victimId,
              hexDto,
            );
      setRobberBusy(false);
      if (r.ok) {
        clearRobberPending();
      } else {
        setRobberErr(r.errorMessage);
      }
    })();
  };

  const handleRoadPicked = (edge: EdgeCoordDto) => {
    if (!gameId) return;

    if (interactionMode === "initialRoad" && pendingInitialVertex) {
      void (async () => {
        const r = await runCmd(() =>
          postPlaceInitial(gameId, pendingInitialVertex, edge),
        );
        if (r.ok) {
          setPendingInitialVertex(null);
          setInteractionMode("idle");
        }
      })();
      return;
    }

    if (interactionMode === "buildRoad") {
      void runCmd(() => postBuildRoad(gameId, edge));
      return;
    }

    if (interactionMode === "devRoad1") {
      setDevRoadFirstEdge(edge);
      setInteractionMode("devRoad2");
      return;
    }

    if (
      interactionMode === "devRoad2" &&
      devRoadFirstEdge &&
      privateGame
    ) {
      void (async () => {
        const r = await runCmd(() =>
          postUseRoadBuilding(
            gameId,
            privateGame.myPlayerId,
            devRoadFirstEdge,
            edge,
          ),
        );
        if (r.ok) resetDevRoad();
      })();
    }
  };

  const handleVertexPicked = (vertex: VertexCoordDto) => {
    if (!gameId) return;

    if (interactionMode === "initialSettle") {
      setPendingInitialVertex(vertex);
      setInteractionMode("initialRoad");
      return;
    }

    if (interactionMode === "buildSettlement") {
      void runCmd(() => postBuildSettlement(gameId, vertex));
    }
  };

  const handleCityPicked = (vertex: VertexCoordDto) => {
    if (!gameId) return;
    if (interactionMode === "upgradeCity") {
      void runCmd(() => postUpgradeCity(gameId, vertex));
    }
  };

  const onPlayKnight = () => {
    setRobberKind("knight");
    setInteractionMode("devKnightHex");
  };

  const onPlayMonopoly = () => {
    setDevErr(null);
    setMonopolyOpen(true);
  };

  const onPlayYearOfPlenty = () => {
    setDevErr(null);
    setYearOpen(true);
  };

  const onPlayRoadBuilding = () => {
    setDevRoadFirstEdge(null);
    setInteractionMode("devRoad1");
  };

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
        <div className="overflow-hidden rounded-xl border border-slate-200 bg-slate-950 shadow-inner">
          <div
            style={{ height: "min(70vh, 640px)", minHeight: 360 }}
            className="w-full"
          >
            <CatanBoardScene
              game={boardGame}
              hexRadius={1}
              boardPickMode={boardPickMode}
              buildableRoads={
                boardView === "live" && privateGame
                  ? interactionMode === "initialRoad" ||
                      interactionMode === "devRoad1" ||
                      interactionMode === "devRoad2"
                    ? undefined
                    : privateGame.buildableRoads
                  : undefined
              }
              buildableSettlements={
                boardView === "live" && privateGame
                  ? privateGame.buildableSettlements
                  : undefined
              }
              initialRoadVertexHexes={
                interactionMode === "initialRoad"
                  ? initialRoadHexes
                  : null
              }
              myPlayerId={privateGame?.myPlayerId}
              onRoadPicked={
                boardView === "live" ? handleRoadPicked : undefined
              }
              onVertexPicked={
                boardView === "live" ? handleVertexPicked : undefined
              }
              onRobberHexPicked={
                boardView === "live" &&
                (interactionMode === "robberHex" ||
                  interactionMode === "devKnightHex")
                  ? handleRobberHexChosen
                  : undefined
              }
              onSettlementCityPicked={
                boardView === "live" ? handleCityPicked : undefined
              }
            />
          </div>
          <GamePlayersTurnBar
            show={showLiveHud}
            game={game}
            myPlayerId={privateGame?.myPlayerId}
          />
          <PlayerInventoryStrip
            show={showLiveHud}
            privateGame={privateGame}
            me={me}
          />
          <GameActionBar
            show={showLiveHud}
            game={game}
            gameId={gameId}
            isMyTurn={isMyTurn}
            hasPrivateSlice={!!privateGame}
            interactionMode={interactionMode}
            onInteractionMode={(m) => {
              if (m === "idle") {
                setPendingInitialVertex(null);
                resetDevRoad();
                clearRobberPending();
              }
              setInteractionMode(m);
            }}
            actionError={actionError}
            onClearActionError={() => setActionError(null)}
            onRollDice={handleRollDice}
            onEndTurn={handleEndTurn}
            onBuyDevCard={handleBuyDevCard}
            onPlayKnight={onPlayKnight}
            onPlayMonopoly={onPlayMonopoly}
            onPlayYearOfPlenty={onPlayYearOfPlenty}
            onPlayRoadBuilding={onPlayRoadBuilding}
            onStartRobber={handleStartRobber}
          />
        </div>
      </div>

      {mustDiscard && privateGame && gameId && (
        <DiscardHalfDialog
          open={mustDiscard}
          gameId={gameId}
          required={me!.discardRequirement}
          privateGame={privateGame}
          onClose={() => {}}
        />
      )}

      <RobberVictimPicker
        open={!!pendingRobberHex && victimChoices.length !== 1}
        victims={victimChoices}
        busy={robberBusy}
        error={robberErr}
        onPick={handleVictimPick}
        onCancel={() => {
          clearRobberPending();
          setRobberErr(null);
        }}
      />

      <DevCardResourceDialog
        mode="monopoly"
        open={monopolyOpen}
        busy={devBusy}
        error={devErr}
        onCancel={() => setMonopolyOpen(false)}
        onConfirm={async (resourceType) => {
          if (!gameId || !privateGame) return;
          setDevBusy(true);
          setDevErr(null);
          const r = await postUseMonopoly(
            gameId,
            privateGame.myPlayerId,
            resourceType,
          );
          setDevBusy(false);
          if (r.ok) setMonopolyOpen(false);
          else setDevErr(r.errorMessage);
        }}
      />

      <DevCardResourceDialog
        mode="yearOfPlenty"
        open={yearOpen}
        busy={devBusy}
        error={devErr}
        onCancel={() => setYearOpen(false)}
        onConfirm={async (r1, r2) => {
          if (!gameId || !privateGame) return;
          setDevBusy(true);
          setDevErr(null);
          const r = await postUseYearOfPlenty(
            gameId,
            privateGame.myPlayerId,
            r1,
            r2,
          );
          setDevBusy(false);
          if (r.ok) setYearOpen(false);
          else setDevErr(r.errorMessage);
        }}
      />

      {import.meta.env.DEV && <GameStoreDebugView />}

    </div>
  );
}

export default GamePage;
