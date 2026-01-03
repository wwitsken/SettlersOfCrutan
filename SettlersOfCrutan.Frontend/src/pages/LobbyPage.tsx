import { useEffect } from "react";
import {
  useLoaderData,
  useNavigate,
  useParams,
  type LoaderFunctionArgs,
} from "react-router";
import { useLobbyStore } from "../stores/lobbyStore";
import { api } from "../api/client";
import { acquireAccessToken } from "../../authConfig";
import { useSignalRContext } from "../context/SignalRContext";
import type { Lobby } from "../domain/lobby/lobby";

export async function LobbyLoader(args: LoaderFunctionArgs) {
  if (!args.params.lobbyId) return { status: 404 };
  const { data, response } = await api.GET("/api/lobby/{lobbyId}", {
    params: {
      path: {
        lobbyId: args.params.lobbyId,
      },
    },
    accessToken: await acquireAccessToken(),
  });
  return { data, loadedStatus: response.status };
}

function LobbyPage() {
  const {
    isConnected,
    isConnecting,
    error: signalRError,
    start,
    registerHandlers,
  } = useSignalRContext();
  const { lobbyId } = useParams() ?? null;
  const navigate = useNavigate();
  const { data, loadedStatus } = useLoaderData<typeof LobbyLoader>();

  const status = useLobbyStore((s) => s.status);
  const error = useLobbyStore((s) => s.error);
  const currentLobby = useLobbyStore((s) => s.currentLobby);
  const setLobby = useLobbyStore((s) => s.setLobby);
  const clearLobby = useLobbyStore((s) => s.clear);

  useEffect(() => {
    if (loadedStatus === 200 && data !== undefined) {
      const newLobby: Lobby = {
        lobbyId: data.lobbyId!,
        lobbyMembers: (data.lobbyMembers ?? []).map((m) => ({
          id: m.id!,
          displayName: m.displayName ?? undefined,
          isMe: !!m.isMe,
          isHost: !!m.isHost,
          isReady: !!m.isReady,
        })),
      };
      setLobby(newLobby);
    }
  }, [loadedStatus, data, setLobby]);

  // Register SignalR handlers once and ensure connection started
  useEffect(() => {
    registerHandlers({
      LobbyReceive: (...args: unknown[]) => {
        const [evtLobbyId, timestamp, eventName, payload] = args as [
          string,
          Date,
          string,
          unknown
        ];
        // Update store from event payload
        try {
          const projected: Lobby = (payload ?? {}) as Lobby;
          setLobby(projected);
          // Optional log for visibility
          console.log(
            `lobbyId: ${evtLobbyId}\n timeStamp: ${timestamp}\n eventName: ${eventName}\n payLoad: ${JSON.stringify(
              projected
            )}`
          );
        } catch (e) {
          console.error("Failed to project LobbyReceive payload:", e);
        }
      },
      MoveFromLobbyToGame: (...args: unknown[]) => {
        const [evtLobbyId, evtGameId, timestamp] = args as [
          string,
          string,
          Date
        ];

        try {
          console.log(
            `MoveFromLobbyToGame detected. lobbyId: ${evtLobbyId}\n gameId: ${evtGameId}\n timeStamp: ${timestamp}\n`
          );
          clearLobby();
          navigate(`/game/${evtGameId}`);
        } catch (e) {
          console.error("Failed to process MoveFromLobbyToGame signal: ", e);
        }
      },
    });
    (async () => {
      try {
        await start();
      } catch (e) {
        console.error("An error occurred: ", e);
        // handled via context error
      }
    })();
  }, [
    registerHandlers,
    start,
    setLobby,
    clearLobby,
    navigate,
    currentLobby?.lobbyId,
  ]);

  const me = currentLobby?.lobbyMembers.find((m) => m.isMe);
  const isInLobby = !!me;
  const isReady = !!me?.isReady;

  if (isConnecting && !isConnected) {
    return <div className="p-4">Connecting to game services...</div>;
  }

  if (signalRError && !isConnected) {
    return (
      <div className="p-4 text-red-600">
        Could not connect to game services: {signalRError}
      </div>
    );
  }

  return (
    <div className="space-y-4">
      <h1 className="text-2xl font-semibold">Crutan Lobby</h1>
      <div className="text-sm text-gray-600">
        Lobby: {currentLobby?.lobbyId ?? lobbyId}
      </div>

      <div className="flex items-center gap-2">
        {!isInLobby && lobbyId && (
          <button
            onClick={async () =>
              await api.POST("/api/lobby/{lobbyId}/join", {
                params: { path: { lobbyId: lobbyId } },
              })
            }
            className="px-3 py-1 cursor-pointer rounded border bg-white hover:bg-gray-50"
            disabled={!lobbyId || status === "loading"}
          >
            {status === "loading" ? "Joining…" : "Join"}
          </button>
        )}
        {isInLobby && lobbyId && (
          <button
            onClick={() => {
              api.POST("/api/lobby/{lobbyId}/leave", {
                params: { path: { lobbyId } },
              });
              navigate("/");
            }}
            className="px-3 py-1 cursor-pointer rounded border bg-white hover:bg-gray-50"
            disabled={status === "loading"}
          >
            {status === "loading" ? "Leaving…" : "Leave"}
          </button>
        )}
        {isInLobby && lobbyId && !isReady && (
          <button
            onClick={async () =>
              await api.POST("/api/lobby/{lobbyId}/ready", {
                params: { path: { lobbyId } },
              })
            }
            className="px-3 py-1  cursor-pointer rounded border bg-green-600 text-white hover:bg-green-700"
            disabled={status === "loading"}
          >
            {status === "loading" ? "Updating…" : "Ready"}
          </button>
        )}
        {isInLobby && lobbyId && isReady && (
          <button
            onClick={async () =>
              await api.POST("/api/lobby/{lobbyId}/unready", {
                params: { path: { lobbyId } },
              })
            }
            className="px-3 py-1  cursor-pointer rounded border bg-yellow-500 text-white hover:bg-yellow-600"
            disabled={status === "loading"}
          >
            {status === "loading" ? "Updating…" : "Unready"}
          </button>
        )}
        {isInLobby &&
          lobbyId &&
          currentLobby?.lobbyMembers.every((m) => m.isReady) && (
            <button
              onClick={async () =>
                await api.POST("/api/lobby/{lobbyId}/start-game", {
                  params: { path: { lobbyId } },
                  body: {
                    gameName: "Crutan Game Example",
                    gameType: "baseGame",
                  },
                })
              }
              className="px-3 py-1  cursor-pointer rounded border bg-green-500 text-white hover:bg-yellow-600"
              disabled={status === "loading"}
            >
              Start Game
            </button>
          )}
      </div>

      {error && <p className="text-sm text-red-600">{error}</p>}

      <ul className="flex flex-col space-y-2">
        {currentLobby?.lobbyMembers?.map((p, idx) => (
          <li key={idx} className="px-3 py-2 bg-white border rounded">
            <div className="flex justify-between">
              <span>{p.displayName || "no name"}</span>
              <span className="text-xs text-gray-500">
                {p.isHost ? "Host" : "Player"} •{" "}
                {p.isReady ? "Ready" : "Not ready"}
                {p.isMe ? " • You" : ""}
              </span>
            </div>
          </li>
        ))}
      </ul>
    </div>
  );
}

export default LobbyPage;
