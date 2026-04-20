import { useCallback, useEffect, useMemo, useRef, useState } from "react";
import {
  useLoaderData,
  useParams,
  type LoaderFunctionArgs,
} from "react-router";
import { CatanBoardScene } from "../components/board/CatanBoardScene";
import { GameStoreDebugView } from "../components/dev/GameStoreDebugView";
import { useGameSignalR } from "../hooks/useGameSignalR";
import { useGamesStore } from "../stores/gameStore";
import { api } from "../api/client";
import { getAccessTokenForOpenApi } from "../authConfig";
import { resolveBoardView } from "../domain/game/boardView";
import { game as exampleGame } from "../domain/game/gameExample";
import { applyGamePayloadFromApi } from "../stores/applyGamePayload";
import { GameActionBar } from "../components/game/GameActionBar";
import { IncomingTradeBar } from "../components/game/IncomingTradeBar";
import { ResourceMaritimePopover } from "../components/game/ResourceMaritimePopover";
import { ProposeTradeDialog } from "../components/game/ProposeTradeDialog";
import { DiscardHalfDialog } from "../components/game/DiscardHalfDialog";
import { RobberVictimPicker } from "../components/game/RobberVictimPicker";
import { DevCardResourceDialog } from "../components/game/DevCardResourceDialog";
import { useGamePageInteraction } from "../hooks/useGamePageInteraction";
import { deriveBoardInteraction } from "../domain/game/boardInteraction";
import { getCurrentPlayer, playersForLayout } from "../domain/game/selectors";
import { getPlayerIdsExposedToHex } from "../domain/game/robberExposure";
import { toHexCoordDto, vertexDtoToHexes } from "../domain/game/hexCoords";
import {
  expandUnplayedDevCards,
  mergePlayedDevCardsFromApi,
} from "../domain/game/devHandUi";
import { bestMaritimeRatio } from "../domain/game/maritimeEligibility";
import type { HexCoordinate } from "../domain/game/board";
import type { DevelopmentCardType } from "../domain/game/gameTypes";
import type { ResourceCardType } from "../domain/game/gameTypes";
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
import { GameBoardToasts } from "../components/game/GameBoardToasts";
import CatanLayout from "../components/layout/CatanLayout";
import { useGameToastStore } from "../stores/gameToastStore";
import type { ChatMessage } from "../domain/game/gameTypes";
import { useUserProfiles } from "../hooks/useUserName";
import { fetchUserProfiles, type UserProfile } from "../api/userProfiles";

type EdgeCoordDto = components["schemas"]["EdgeCoordDto"];
type VertexCoordDto = components["schemas"]["VertexCoordDto"];

const MOCK_CHAT_MESSAGES: ChatMessage[] = [
  {
    id: 1,
    player: "System",
    color: "none",
    text: "Game started. Chat coming soon.",
    time: "",
  },
];

// ── Loader ──────────────────────────────────────────────────────

type GameDto = components["schemas"]["GameDto"];

type GameLoaderResult = {
  loadedStatus: number;
  gameData: GameDto | null;
  users: UserProfile[];
};

export async function GameLoader(
  args: LoaderFunctionArgs,
): Promise<GameLoaderResult> {
  if (!args.params.gameId)
    return { loadedStatus: 404, gameData: null, users: [] };

  const { data: gameData, response: gameResponse } = await api.GET(
    "/api/games/{id}",
    {
      params: { path: { id: args.params.gameId } },
      accessToken: (await getAccessTokenForOpenApi()) ?? "",
    },
  );

  if (gameResponse.status !== 200 || !gameData) {
    return { loadedStatus: gameResponse.status, gameData: null, users: [] };
  }

  const userIds = (gameData.game?.players ?? [])
    .map((p) => p.userId)
    .filter((id): id is string => typeof id === "string" && id.length > 0);
  const users = await fetchUserProfiles(userIds);

  return { loadedStatus: 200, gameData, users };
}

// ── Page ────────────────────────────────────────────────────────

function GamePage() {
  const { gameId } = useParams();
  const {
    loadedStatus,
    gameData,
    users: initialUsers,
  } = useLoaderData<typeof GameLoader>();
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
  const [maritimePopover, setMaritimePopover] = useState<{
    discard: ResourceCardType;
    anchorEl: HTMLButtonElement;
  } | null>(null);
  const [handHint, setHandHint] = useState<string | null>(null);
  const [devBusy, setDevBusy] = useState(false);
  const [devErr, setDevErr] = useState<string | null>(null);
  const [robberBusy, setRobberBusy] = useState(false);
  const [robberErr, setRobberErr] = useState<string | null>(null);

  const { users, fetchProfiles } = useUserProfiles(initialUsers);

  // Fetch user profiles once per unique set of players (not on every SignalR tick).
  // Seed the cache key with the ids the loader already fetched so the initial
  // render does not re-request them.
  const initialFetchKey = useMemo(
    () =>
      [...initialUsers.map((u) => u.userId)].sort().join(","),
    [initialUsers],
  );
  const fetchedForRef = useRef(initialFetchKey);
  useEffect(() => {
    if (!game?.players.length) return;
    const userIds = game.players.map((p) => p.userId).filter(Boolean);
    const key = [...userIds].sort().join(",");
    if (key === fetchedForRef.current) return;
    fetchedForRef.current = key;
    void fetchProfiles(userIds);
  }, [game, fetchProfiles]);

  // Merge fetched display names into a derived game object used for rendering.
  const enrichedGame = useMemo(() => {
    if (!game) return null;
    if (users.length === 0) return game;
    const byUserId = new Map(users.map((u) => [u.userId, u]));
    return {
      ...game,
      players: game.players.map((p) => ({
        ...p,
        displayName: byUserId.get(p.userId)?.displayName ?? p.displayName,
      })),
    };
  }, [game, users]);

  useGameSignalR(gameId ?? null);

  const clearGameToasts = useGameToastStore((s) => s.clear);
  useEffect(() => {
    clearGameToasts();
  }, [gameId, clearGameToasts]);

  useEffect(() => {
    if (!gameId) return;
    setLoading(gameId);

    if (loadedStatus !== 200 || !gameData) {
      setError(`Game request failed (${loadedStatus}).`);
      return;
    }

    if (!applyGamePayloadFromApi(gameData))
      setError("Game response could not be mapped.");
  }, [gameId, loadedStatus, gameData, setLoading, setError]);

  useEffect(() => {
    if (!handHint) return;
    const t = window.setTimeout(() => setHandHint(null), 3200);
    return () => window.clearTimeout(t);
  }, [handHint]);

  const boardView = resolveBoardView(status, game);
  const boardGame =
    boardView === "loading" ? exampleGame : (game ?? exampleGame);
  const showLoadingBanner = boardView === "loading";
  const showExampleBanner = boardView === "example";

  const me =
    enrichedGame && privateGame ? getCurrentPlayer(enrichedGame, privateGame) : undefined;
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

  const blockForRobberFlow =
    !!(isMyTurn && game?.gamePhase === "resolveRobber") ||
    awaitingKnightRobberHex;
  const toolbarIdle = !devRoadPicking && !blockForRobberFlow;
  const resourceMaritimeEnabled =
    showLiveHud &&
    isMyTurn &&
    game?.gamePhase === "tradeBuild" &&
    toolbarIdle &&
    !!privateGame;
  const unplayedDevPlayEnabled = resourceMaritimeEnabled;

  const incomingTrade =
    showLiveHud &&
    privateGame &&
    game!.currentTradeOffer &&
    game!.currentTradeOffer.playerProposerId !== privateGame.myPlayerId
      ? game!.currentTradeOffer
      : undefined;

  const tradeProposerName = useMemo(() => {
    if (!incomingTrade || !enrichedGame) return "";
    return (
      enrichedGame.players.find((p) => p.id === incomingTrade.playerProposerId)
        ?.displayName || incomingTrade.playerProposerId
    );
  }, [incomingTrade, enrichedGame]);

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
        displayName: enrichedGame?.players.find((p) => p.id === id)?.displayName || id,
      }));
  }, [game, privateGame, pendingRobberHex, enrichedGame]);

  const layoutPlayers = useMemo(
    () => (enrichedGame ? playersForLayout(enrichedGame) : []),
    [enrichedGame],
  );
  const layoutUnplayedDevCards = useMemo(
    () => expandUnplayedDevCards(privateGame?.myHand.devCards ?? {}),
    [privateGame],
  );
  const layoutPlayedDevCards = useMemo(
    () => mergePlayedDevCardsFromApi(privateGame?.playedDevelopmentCards),
    [privateGame],
  );

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

  const handleUnplayedDevCardClick = (type: DevelopmentCardType) => {
    if (!unplayedDevPlayEnabled) return;
    switch (type) {
      case "knight":
        onPlayKnight();
        break;
      case "monopoly":
        onPlayMonopoly();
        break;
      case "yearOfPlenty":
        onPlayYearOfPlenty();
        break;
      case "roadBuilding":
        onPlayRoadBuilding();
        break;
      case "victoryPoint":
        setHandHint("Victory point cards cannot be played.");
        break;
      default:
        break;
    }
  };

  const handleResourceCardMaritime = (
    discard: ResourceCardType,
    anchorEl: HTMLButtonElement,
  ) => {
    if (!resourceMaritimeEnabled) return;
    setMaritimePopover({ discard, anchorEl });
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
      <div className="flex-1 min-h-0 relative">
        <GameBoardToasts />
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
      onProposeTrade={() => setProposeTradeOpen(true)}
    />
  );

  return (
    <>
      {loadError && (
        <div
          className="px-4 py-2 text-sm text-red-300 bg-red-900/20 border-b border-red-700/40"
          role="alert"
        >
          {loadError}
        </div>
      )}

      {handHint && (
        <div
          className="px-4 py-2 text-sm text-stone-200 bg-stone-800 border-b border-stone-600"
          role="status"
        >
          {handHint}
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
        resourceMaritimeEnabled={resourceMaritimeEnabled}
        onResourceCardMaritime={handleResourceCardMaritime}
        unplayedDevPlayEnabled={unplayedDevPlayEnabled}
        onUnplayedDevCardClick={handleUnplayedDevCardClick}
      />

      {proposeTradeOpen && privateGame && gameId && (
        <ProposeTradeDialog
          open={proposeTradeOpen}
          gameId={gameId}
          privateGame={privateGame}
          onClose={() => setProposeTradeOpen(false)}
        />
      )}

      {maritimePopover && game && privateGame && gameId && (
        <ResourceMaritimePopover
          gameId={gameId}
          game={game}
          privateGame={privateGame}
          discard={maritimePopover.discard}
          ratio={bestMaritimeRatio(
            game,
            privateGame.myPlayerId,
            maritimePopover.discard,
            privateGame.myHand.resources[maritimePopover.discard] ?? 0,
          )}
          anchorEl={maritimePopover.anchorEl}
          onClose={() => setMaritimePopover(null)}
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
