import { useState } from "react";
import { Link, useNavigate } from "react-router";
import { useLobbyStore } from "../stores/lobbyStore";
import { useAuthStore } from "../stores/authStore";

function Home() {
  const [lobby, setLobby] = useState<string>("");
  const navigate = useNavigate();
  const createLobby = useLobbyStore((s) => s.createLobby);
  const joinLobby = useLobbyStore((s) => s.joinLobby);
  const status = useLobbyStore((s) => s.status);
  const error = useLobbyStore((s) => s.error);
  const isAuthed = useAuthStore((s) => s.status === "authenticated");
  return (
    <div className="bg-gray-100 flex flex-col items-center justify-center">
      <h1 className="text-4xl font-bold text-blue-600 mb-6">
        Settlers of Crutan
      </h1>
      {isAuthed && (
        <>
          <button
            onClick={async () => {
              const id = await createLobby();
              if (id) navigate(`/lobby/${id}`);
            }}
            className="mb-4 px-3 py-2 border rounded bg-white hover:bg-gray-50"
            disabled={status === "loading"}
          >
            {status === "loading" ? "Creating…" : "Create a game"}
          </button>
          <div className="flex flex-row space-x-2">
            <input
              value={lobby}
              className="p-1 border border-gray-400 rounded-sm"
              onChange={(e) => setLobby(e.target.value)}
            ></input>
            <button
              onClick={async () => {
                if (!lobby) return;
                await joinLobby(lobby);
                navigate(`/lobby/${lobby}`);
              }}
              className="p-1 border rounded-sm border-gray-600 bg-white hover:bg-gray-50"
              disabled={!lobby || status === "loading"}
            >
              {status === "loading" ? "Joining…" : "Join lobby"}
            </button>
            <Link to={`lobby/${lobby}`} className="p-1 underline text-blue-700">
              Open lobby page
            </Link>
          </div>
          <h2>My Games</h2>
          <ul>
            <ol>Game 1</ol>
            <ol>Game 2</ol>
          </ul>
        </>
      )}
      {!isAuthed && <p>You gotta log in, bud</p>}
      {error && <p className="mt-2 text-sm text-red-600">{error}</p>}
    </div>
  );
}

export default Home;
