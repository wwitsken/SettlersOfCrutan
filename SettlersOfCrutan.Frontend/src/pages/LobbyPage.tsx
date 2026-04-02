import { useEffect, useState } from "react";
import {
  useLoaderData,
  useNavigate,
  useParams,
  type LoaderFunctionArgs,
} from "react-router";
import { useLobbyStore } from "../stores/lobbyStore";
import { api } from "../api/client";
import { getAccessTokenForOpenApi } from "../authConfig";
import { useSignalRContext } from "../context/SignalRContext";
import { lobbyDtoToDomain } from "../domain/lobby/mapLobbyDto";
import { lobbyFullStateEvents } from "../realtime/realtimeEvents";
import type { components } from "../api/types";

export async function LobbyLoader(args: LoaderFunctionArgs) {
  if (!args.params.lobbyId) return { status: 404 };
  const { data, response } = await api.GET("/api/lobby/{lobbyId}", {
    params: {
      path: {
        lobbyId: args.params.lobbyId,
      },
    },
    accessToken: (await getAccessTokenForOpenApi()) ?? "",
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
  const [copyHint, setCopyHint] = useState<string | null>(null);

  const status = useLobbyStore((s) => s.status);
  const error = useLobbyStore((s) => s.error);
  const currentLobby = useLobbyStore((s) => s.currentLobby);
  const setLobby = useLobbyStore((s) => s.setLobby);
  const clearLobby = useLobbyStore((s) => s.clear);

  useEffect(() => {
    if (loadedStatus === 200 && data !== undefined) {
      const mapped = lobbyDtoToDomain(data as components["schemas"]["LobbyDto"]);
      if (mapped) setLobby(mapped);
    }
  }, [loadedStatus, data, setLobby]);

  useEffect(() => {
    registerHandlers({
      LobbyReceive: (...args: unknown[]) => {
        const [, , eventName, payload] = args as [
          string,
          Date | string,
          string,
          unknown,
        ];
        if (!lobbyFullStateEvents.has(eventName)) {
          if (import.meta.env.DEV) {
            console.debug(
              "[SignalR] LobbyReceive ignored (unknown event)",
              eventName,
            );
          }
          return;
        }
        try {
          const projected = lobbyDtoToDomain(
            (payload ?? {}) as components["schemas"]["LobbyDto"],
          );
          if (projected) setLobby(projected);
        } catch (e) {
          console.error("Failed to project LobbyReceive payload:", e);
        }
      },
      MoveFromLobbyToGame: (...args: unknown[]) => {
        const [, evtGameId] = args as [string, string, Date];
        try {
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
      }
    })();
  }, [registerHandlers, start, setLobby, clearLobby, navigate]);

  const me = currentLobby?.lobbyMembers.find((m) => m.isMe);
  const isInLobby = !!me;
  const isReady = !!me?.isReady;
  const activeLobbyId = currentLobby?.lobbyId ?? lobbyId;

  const copyLobbyCode = async () => {
    if (!activeLobbyId) return;
    try {
      await navigator.clipboard.writeText(activeLobbyId);
      setCopyHint("Copied to clipboard");
      setTimeout(() => setCopyHint(null), 2000);
    } catch {
      setCopyHint("Could not copy");
      setTimeout(() => setCopyHint(null), 2000);
    }
  };

  if (isConnecting && !isConnected) {
    return (
      <div className="rounded-xl border border-slate-200 bg-white p-6 text-slate-600 shadow-sm">
        Connecting to game services…
      </div>
    );
  }

  if (signalRError && !isConnected) {
    return (
      <div
        className="rounded-xl border border-red-200 bg-red-50 p-6 text-red-800"
        role="alert"
      >
        Could not connect to game services: {signalRError}
      </div>
    );
  }

  return (
    <div className="mx-auto max-w-2xl space-y-6">
      <div className="rounded-2xl border border-slate-200 bg-white p-6 shadow-sm">
        <h1 className="text-2xl font-semibold tracking-tight text-slate-900">
          Lobby
        </h1>
        <p className="mt-1 text-sm text-slate-500">
          Share this code with other players so they can join from the home
          page.
        </p>
        <div className="mt-4 flex flex-wrap items-center gap-2">
          <code className="rounded-lg bg-slate-100 px-3 py-2 font-mono text-sm text-slate-800">
            {activeLobbyId ?? "—"}
          </code>
          <button
            type="button"
            onClick={() => void copyLobbyCode()}
            className="rounded-lg border border-slate-300 bg-white px-3 py-2 text-sm font-medium text-slate-700 hover:bg-slate-50"
          >
            Copy code
          </button>
          {copyHint && (
            <span className="text-sm text-emerald-600">{copyHint}</span>
          )}
        </div>
      </div>

      <div className="flex flex-wrap items-center gap-2">
        {!isInLobby && lobbyId && (
          <button
            type="button"
            onClick={async () =>
              await api.POST("/api/lobby/{lobbyId}/join", {
                params: { path: { lobbyId: lobbyId } },
              })
            }
            className="rounded-lg border border-slate-300 bg-white px-4 py-2 text-sm font-medium hover:bg-slate-50 disabled:opacity-50"
            disabled={!lobbyId || status === "loading"}
          >
            {status === "loading" ? "Joining…" : "Join lobby"}
          </button>
        )}
        {isInLobby && lobbyId && (
          <button
            type="button"
            onClick={() => {
              void api.POST("/api/lobby/{lobbyId}/leave", {
                params: { path: { lobbyId } },
              });
              navigate("/");
            }}
            className="rounded-lg border border-slate-300 bg-white px-4 py-2 text-sm font-medium hover:bg-slate-50 disabled:opacity-50"
            disabled={status === "loading"}
          >
            {status === "loading" ? "Leaving…" : "Leave"}
          </button>
        )}
        {isInLobby && lobbyId && !isReady && (
          <button
            type="button"
            onClick={async () =>
              await api.POST("/api/lobby/{lobbyId}/ready", {
                params: { path: { lobbyId } },
              })
            }
            className="rounded-lg bg-emerald-600 px-4 py-2 text-sm font-medium text-white hover:bg-emerald-700 disabled:opacity-50"
            disabled={status === "loading"}
          >
            {status === "loading" ? "Updating…" : "Ready"}
          </button>
        )}
        {isInLobby && lobbyId && isReady && (
          <button
            type="button"
            onClick={async () =>
              await api.POST("/api/lobby/{lobbyId}/unready", {
                params: { path: { lobbyId } },
              })
            }
            className="rounded-lg bg-amber-500 px-4 py-2 text-sm font-medium text-white hover:bg-amber-600 disabled:opacity-50"
            disabled={status === "loading"}
          >
            {status === "loading" ? "Updating…" : "Unready"}
          </button>
        )}
        {isInLobby &&
          lobbyId &&
          currentLobby?.lobbyMembers.length &&
          currentLobby.lobbyMembers.every((m) => m.isReady) && (
            <button
              type="button"
              onClick={async () =>
                await api.POST("/api/lobby/{lobbyId}/start-game", {
                  params: { path: { lobbyId } },
                  body: {
                    gameName: "Crutan Game",
                    gameType: "baseGame",
                  },
                })
              }
              className="rounded-lg bg-emerald-700 px-4 py-2 text-sm font-medium text-white hover:bg-emerald-800 disabled:opacity-50"
              disabled={status === "loading"}
            >
              Start game
            </button>
          )}
      </div>

      {error && (
        <p className="text-sm text-red-600" role="alert">
          {error}
        </p>
      )}

      <ul className="space-y-2">
        {currentLobby?.lobbyMembers?.map((p) => (
          <li
            key={p.id}
            className="rounded-xl border border-slate-200 bg-white px-4 py-3 shadow-sm"
          >
            <div className="flex justify-between gap-2">
              <span className="font-medium text-slate-900">
                {p.displayName || "Player"}
              </span>
              <span className="text-xs text-slate-500">
                {p.isHost ? "Host" : "Player"} ·{" "}
                {p.isReady ? "Ready" : "Not ready"}
                {p.isMe ? " · You" : ""}
              </span>
            </div>
          </li>
        ))}
      </ul>
    </div>
  );
}

export default LobbyPage;
