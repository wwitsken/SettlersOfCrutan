import { useEffect, useMemo } from "react";
import { useParams } from "react-router";
import { useLobbyStore } from "../lobbies/store";

function Lobby() {
  const params = useParams();
  const lobbyId = params.lobbyId ?? null;

  const status = useLobbyStore((s) => s.status);
  const error = useLobbyStore((s) => s.error);
  const currentLobby = useLobbyStore((s) => s.currentLobby);
  const loadLobby = useLobbyStore((s) => s.loadLobby);
  const joinLobby = useLobbyStore((s) => s.joinLobby);
  const leaveLobby = useLobbyStore((s) => s.leaveLobby);
  const setReady = useLobbyStore((s) => s.setReady);
  const setUnready = useLobbyStore((s) => s.setUnready);
  const startRealtime = useLobbyStore((s) => s.startRealtime);
  const stopRealtime = useLobbyStore((s) => s.stopRealtime);

  useEffect(() => {
    if (lobbyId) loadLobby(lobbyId);
  }, [lobbyId, loadLobby]);

  useEffect(() => {
    if (!lobbyId) return;
    startRealtime(lobbyId);
    return () => {
      stopRealtime();
    };
  }, [lobbyId, startRealtime, stopRealtime]);

  const me = useMemo(
    () => currentLobby?.lobbyPlayers?.find((p) => p.isMe),
    [currentLobby]
  );
  const isInLobby = !!me;
  const isReady = !!me?.isReady;

  return (
    <div className="space-y-4">
      <h1 className="text-2xl font-semibold">Catan Lobby</h1>
      <div className="text-sm text-gray-600">
        Lobby: {currentLobby?.lobbyId ?? lobbyId}
      </div>

      <div className="flex items-center gap-2">
        {!isInLobby && lobbyId && (
          <button
            onClick={async () => lobbyId && (await joinLobby(lobbyId))}
            className="px-3 py-1 rounded border bg-white hover:bg-gray-50"
            disabled={!lobbyId || status === "loading"}
          >
            {status === "loading" ? "Joining…" : "Join"}
          </button>
        )}
        {isInLobby && lobbyId && (
          <button
            onClick={async () => await leaveLobby(lobbyId)}
            className="px-3 py-1 rounded border bg-white hover:bg-gray-50"
            disabled={status === "loading"}
          >
            {status === "loading" ? "Leaving…" : "Leave"}
          </button>
        )}
        {isInLobby && lobbyId && !isReady && (
          <button
            onClick={async () => await setReady(lobbyId)}
            className="px-3 py-1 rounded border bg-green-600 text-white hover:bg-green-700"
            disabled={status === "loading"}
          >
            {status === "loading" ? "Updating…" : "Ready"}
          </button>
        )}
        {isInLobby && lobbyId && isReady && (
          <button
            onClick={async () => await setUnready(lobbyId)}
            className="px-3 py-1 rounded border bg-yellow-500 text-white hover:bg-yellow-600"
            disabled={status === "loading"}
          >
            {status === "loading" ? "Updating…" : "Unready"}
          </button>
        )}
      </div>

      {error && <p className="text-sm text-red-600">{error}</p>}

      <ul className="flex flex-col space-y-2">
        {currentLobby?.lobbyPlayers?.map((p, idx) => (
          <li key={idx} className="px-3 py-2 bg-white border rounded">
            <div className="flex justify-between">
              <span>{p.gameName || "no name"}</span>
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

export default Lobby;
