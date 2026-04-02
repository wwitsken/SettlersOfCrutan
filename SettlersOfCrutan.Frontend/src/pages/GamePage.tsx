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
import { IncomingTradeBar } from "../components/game/IncomingTradeBar";
import {
  MaritimeTradeDialog,
  type MaritimeRatio,
} from "../components/game/MaritimeTradeDialog";
import { ProposeTradeDialog } from "../components/game/ProposeTradeDialog";
import { DiscardHalfDialog } from "../components/game/DiscardHalfDialog";
import { RobberVictimPicker } from "../components/game/RobberVictimPicker";
import { DevCardResourceDialog } from "../components/game/DevCardResourceDialog";
import { useGamePageInteraction } from "../hooks/useGamePageInteraction";
import { deriveBoardInteraction } from "../domain/game/boardInteraction";
import { getCurrentPlayer, playersForLayout } from "../domain/game/selectors";
import { getPlayerIdsExposedToHex } from "../domain/game/robberExposure";
import { toHexCoordDto, vertexDtoToHexes } from "../domain/game/hexCoords";
import { expandUnplayedDevCards, emptyPlayedDevCards } from "../domain/game/devHandUi";
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
import CatanLayout from "../components/layout/CatanLayout";
import type { ChatMessage } from "../types/catan";

type EdgeCoordDto = components["schemas"]["EdgeCoordDto"];
type VertexCoordDto = components["schemas"]["VertexCoordDto"];

const MOCK_CHAT_MESSAGES: ChatMessage[] = [
  { id: 1, player: "System", color: "none", text: "Game started. Chat coming soon.", time: "" },
];

function GamePage() {
  const { gameId } = useParams();
  const game = useGamesStore((s) => s.game);
  const privateGame = useGamesStore((s) => s.privateGame);
  const setLoading = useGamesStore((s) => s.setLoading);
  const setError = useGamesStore((s) => s.setError);
  const status = useGamesStore((s) => s.status);
  const loadError = useGamesStore((s) => s.error);

  const {
    pendingInitialVertex,
    setPendingInitialVertex,
    pendingRobberHex,
    setPendingRobberHex,
    awaitingKnightRobberHex,
    setAwaitingKnightRobberHex,
    devRoadFlow,
    setDevRoadFlow,
    devRoadFirstEdge,
    setDevRoadFirstEdge,
    devRoadPicking,
    actionError,
    setActionError,
    clearRobberPending,
    resetDevRoad,
    startDevRoadFlow,
  } = useGamePageInteraction();

  const [monopolyOpen, setMonopolyOpen] = useState(false);
  const [yearOpen, setYearOpen] = useState(false);
  const [proposeTradeOpen, setProposeTradeOpen] = useState(false);
  const [maritimeOpen, setMaritimeOpen] = useState(false);
  const [maritimeRatio, setMaritimeRatio] = useState<MaritimeRatio>(4);
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
  const showLoadingBanner = boardView === "loading";
  const showExampleBanner = boardView === "example";

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

  const initialRoadHexes = useMemo(
    () =>
      pendingInitialVertex ? vertexDtoToHexes(pendingInitialVertex) : null,
    [pendingInitialVertex],
  );

  const boardInteraction = useMemo(
    () =>
      deriveBoardInteraction({
        boardViewLive: boardView === "live",
        game,
        privateGame,
        isMyTurn,
        pendingInitialVertex,
        pendingRobberHex,
        awaitingKnightRobberHex,
        devRoadPicking,
        initialRoadVertexHexes: initialRoadHexes,
      }),
    [
      boardView,
      game,
      privateGame,
      isMyTurn,
      pendingInitialVertex,
      pendingRobberHex,
      awaitingKnightRobberHex,
      devRoadPicking,
      initialRoadHexes,
    ],
  );

  const showLiveHud = boardView === "live" && !!game && !!gameId;

  const incomingTrade =
    showLiveHud &&
    privateGame &&
    game!.currentTradeOffer &&
    game!.currentTradeOffer.playerProposerId !== privateGame.myPlayerId
      ? game!.currentTradeOffer
      : undefined;

  const tradeProposerName = useMemo(() => {
    if (!incomingTrade || !game) return "";
    return (
      game.players.find((p) => p.id === incomingTrade.playerProposerId)
        ?.displayName ?? incomingTrade.playerProposerId
    );
  }, [incomingTrade, game]);

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

  const layoutPlayers = useMemo(
    () => (game ? playersForLayout(game) : []),
    [game],
  );
  const layoutUnplayedDevCards = useMemo(
    () => expandUnplayedDevCards(privateGame?.myHand.devCards ?? {}),
    [privateGame],
  );
  const layoutPlayedDevCards = useMemo(() => emptyPlayedDevCards(), []);

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

  const robberCommandContext = boardInteraction.robberCommandContext;

  const handleRobberHexChosen = useCallback(
    (hex: HexCoordinate) => {
      if (!game || !privateGame || !gameId || !robberCommandContext) return;

      const victims = getPlayerIdsExposedToHex(game, hex).filter(
        (id) => id !== privateGame.myPlayerId,
      );

      const finish = async (victimId: string | null) => {
        setRobberBusy(true);
        setRobberErr(null);
        const hexDto = toHexCoordDto(hex);
        const r = await postResolveRobber(gameId, hexDto, victimId);
        setRobberBusy(false);
        if (r.ok) {
          clearRobberPending();
        } else {
          setRobberErr(r.errorMessage);
        }
      };

      if (victims.length === 0) {
        void finish(null);
        return;
      }

      if (victims.length === 1) {
        void finish(victims[0]!);
        return;
      }

      setPendingRobberHex(hex);
    },
    [
      game,
      privateGame,
      gameId,
      robberCommandContext,
      clearRobberPending,
      setPendingRobberHex,
    ],
  );

  const handleVictimPick = (victimId: string) => {
    if (!pendingRobberHex || !gameId) return;
    void (async () => {
      setRobberBusy(true);
      setRobberErr(null);
      const hexDto = toHexCoordDto(pendingRobberHex);
      const r = await postResolveRobber(gameId, hexDto, victimId);
      setRobberBusy(false);
      if (r.ok) {
        clearRobberPending();
      } else {
        setRobberErr(r.errorMessage);
      }
    })();
  };

  const handleRoadPicked = (edge: EdgeCoordDto) => {
    if (!gameId || !game) return;

    if (game.gamePhase === "setup" && pendingInitialVertex) {
      void (async () => {
        const r = await runCmd(() =>
          postPlaceInitial(gameId, pendingInitialVertex, edge),
        );
        if (r.ok) {
          setPendingInitialVertex(null);
        }
      })();
      return;
    }

    if (devRoadFlow === "pickFirst") {
      setDevRoadFirstEdge(edge);
      setDevRoadFlow("pickSecond");
      return;
    }

    if (devRoadFlow === "pickSecond" && devRoadFirstEdge && privateGame) {
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
      return;
    }

    if (game.gamePhase === "tradeBuild") {
      void runCmd(() => postBuildRoad(gameId, edge));
    }
  };

  const handleVertexPicked = (vertex: VertexCoordDto) => {
    if (!gameId || !game) return;

    if (game.gamePhase === "setup" && !pendingInitialVertex) {
      setPendingInitialVertex(vertex);
      return;
    }

    if (game.gamePhase === "tradeBuild") {
      void runCmd(() => postBuildSettlement(gameId, vertex));
    }
  };

  const handleCityPicked = (vertex: VertexCoordDto) => {
    if (!gameId || !game) return;
    if (game.gamePhase === "tradeBuild") {
      void runCmd(() => postUpgradeCity(gameId, vertex));
    }
  };

  const onPlayKnight = () => {
    if (!gameId) return;
    void (async () => {
      setRobberBusy(true);
      setRobberErr(null);
      const r = await postUseKnight(gameId);
      setRobberBusy(false);
      if (r.ok) {
        setAwaitingKnightRobberHex(true);
      } else {
        setRobberErr(r.errorMessage);
      }
    })();
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
    startDevRoadFlow();
  };

  const boardSlot = (
    <>
      {(showLoadingBanner || showExampleBanner) && (
        <div
          className={`shrink-0 px-3 py-1.5 text-center text-xs font-medium ${
            showLoadingBanner
              ? "bg-sky-900/40 text-sky-300"
              : "bg-amber-900/30 text-amber-300"
          }`}
          role="status"
        >
          {showLoadingBanner
            ? "Loading game… (board preview is static example data)"
            : "Example board — store has no loaded game yet."}
        </div>
      )}
      <div className="flex-1 min-h-0">
        <CatanBoardScene
          game={boardGame}
          hexRadius={1}
          boardPickMode={boardInteraction.boardPickMode}
          showGhostRoads={boardInteraction.showGhostRoads}
          showGhostSettlementVertices={
            boardInteraction.showGhostSettlementVertices
          }
          showGhostCityUpgrades={boardInteraction.showGhostCityUpgrades}
          buildableRoads={boardInteraction.buildableRoads}
          buildableSettlements={boardInteraction.buildableSettlements}
          initialRoadVertexHexes={boardInteraction.initialRoadVertexHexes}
          myPlayerId={privateGame?.myPlayerId}
          onRoadPicked={boardView === "live" ? handleRoadPicked : undefined}
          onVertexPicked={boardView === "live" ? handleVertexPicked : undefined}
          onRobberHexPicked={
            boardView === "live" && boardInteraction.allowRobberHexPick
              ? handleRobberHexChosen
              : undefined
          }
          onSettlementCityPicked={
            boardView === "live" ? handleCityPicked : undefined
          }
        />
      </div>
    </>
  );

  const setupAwaitingInitialRoad =
    !!game &&
    game.gamePhase === "setup" &&
    isMyTurn &&
    pendingInitialVertex != null;

  const actionBarSlot = (
    <GameActionBar
      show={showLiveHud}
      game={game}
      gameId={gameId}
      isMyTurn={isMyTurn}
      hasPrivateSlice={!!privateGame}
      setupAwaitingInitialRoad={setupAwaitingInitialRoad}
      devRoadPicking={devRoadPicking}
      awaitingKnightRobberHex={awaitingKnightRobberHex}
      actionError={actionError}
      onClearActionError={() => setActionError(null)}
      onRollDice={handleRollDice}
      onEndTurn={handleEndTurn}
      onBuyDevCard={handleBuyDevCard}
      onPlayKnight={onPlayKnight}
      onPlayMonopoly={onPlayMonopoly}
      onPlayYearOfPlenty={onPlayYearOfPlenty}
      onPlayRoadBuilding={onPlayRoadBuilding}
      onProposeTrade={() => setProposeTradeOpen(true)}
      onMaritimeTrade={(ratio) => {
        setMaritimeRatio(ratio);
        setMaritimeOpen(true);
      }}
    />
  );

  return (
    <>
      <div className="flex flex-wrap items-center justify-between gap-2 px-4 py-2 bg-stone-900 border-b border-stone-700/60 text-sm">
        <div>
          <span className="font-semibold text-stone-100">
            {game?.gameName ?? "Game"}
          </span>
          {gameId && (
            <span className="ml-2 text-xs text-stone-500">
              Id {gameId}
              {game && (
                <>
                  {" · Phase "}
                  <span className="text-stone-300">{game.gamePhase}</span>
                  {" · Round "}
                  {game.round}
                </>
              )}
            </span>
          )}
        </div>
        <div className="text-xs text-stone-500">
          {isConnecting && !isConnected && <span>Connecting to hub…</span>}
          {hubError && !isConnected && (
            <span className="text-red-400">Hub: {hubError}</span>
          )}
          {isConnected && <span className="text-emerald-400">Live updates</span>}
        </div>
      </div>

      {loadError && (
        <div
          className="px-4 py-2 text-sm text-red-300 bg-red-900/20 border-b border-red-700/40"
          role="alert"
        >
          {loadError}
        </div>
      )}

      {incomingTrade && privateGame && gameId && (
        <IncomingTradeBar
          show
          gameId={gameId}
          offer={incomingTrade}
          proposerDisplayName={tradeProposerName}
          privateGame={privateGame}
        />
      )}

      <CatanLayout
        players={layoutPlayers}
        chatMessages={MOCK_CHAT_MESSAGES}
        resourceHand={privateGame?.myHand.resources ?? {}}
        unplayedDevCards={layoutUnplayedDevCards}
        playedDevCards={layoutPlayedDevCards}
        currentPlayerName={me?.displayName ?? ""}
        currentPlayerColor={me?.playerColor ?? "none"}
        boardSlot={boardSlot}
        actionBarSlot={actionBarSlot}
      />

      {proposeTradeOpen && privateGame && gameId && (
        <ProposeTradeDialog
          open={proposeTradeOpen}
          gameId={gameId}
          privateGame={privateGame}
          onClose={() => setProposeTradeOpen(false)}
        />
      )}

      {maritimeOpen && game && privateGame && gameId && (
        <MaritimeTradeDialog
          open={maritimeOpen}
          ratio={maritimeRatio}
          gameId={gameId}
          game={game}
          privateGame={privateGame}
          onClose={() => setMaritimeOpen(false)}
        />
      )}

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
        open={!!pendingRobberHex && victimChoices.length > 1}
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
    </>
  );
}

export default GamePage;
