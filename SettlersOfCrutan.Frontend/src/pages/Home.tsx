import { useState } from "react";
import { Link, useNavigate } from "react-router";
import { useLobbyStore } from "../lobbies/store";

function Home() {
  const [lobby, setLobby] = useState<string>("");
  const navigate = useNavigate();
  const createLobby = useLobbyStore((s) => s.createLobby);
  const joinLobby = useLobbyStore((s) => s.joinLobby);
  const status = useLobbyStore((s) => s.status);
  const error = useLobbyStore((s) => s.error);
  return (
    <div className="bg-gray-100 flex flex-col items-center justify-center">
      <h1 className="text-4xl font-bold text-blue-600 mb-6">
        Settlers of Crutan
      </h1>
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
      {error && <p className="mt-2 text-sm text-red-600">{error}</p>}
      <h2>My Games</h2>
      <ul>
        <ol>Game 1</ol>
        <ol>Game 2</ol>
      </ul>
    </div>
  );
}

/*
function Home() {
  const [statusText, setStatusText] = useState("");
  const [inputText, setInputText] = useState("");
  const [echoResponse, setEchoResponse] = useState("");

  return (
    <div className="bg-gray-100 flex flex-col items-center justify-center">
      <h1 className="text-4xl font-bold text-blue-600 mb-6">Vite + React</h1>
      <div className="card bg-white shadow-lg rounded-lg p-8 flex flex-col items-center">
        <button
          className="bg-blue-500 hover:bg-blue-600 text-white font-semibold py-2 px-4 rounded transition-colors mb-4"
          onClick={async () => {
            try {
              const res = await fetch("api/health");
              const txt = await res.text();
              setStatusText(txt);
            } catch (e: unknown) {
              const msg = e instanceof Error ? e.message : String(e);
              setStatusText(`Error: ${msg}`);
            }
          }}
        >
          Api Status: {statusText}
        </button>

        <div className="w-full max-w-md space-y-2">
          <label
            htmlFor="echo-input"
            className="block text-sm font-medium text-gray-700"
          >
            Enter text
          </label>
          <input
            id="echo-input"
            type="text"
            className="w-full border rounded px-3 py-2 focus:outline-none focus:ring-2 focus:ring-blue-500"
            placeholder="Type something..."
            value={inputText}
            onChange={(e) => setInputText(e.target.value)}
          />

          <button
            className="bg-green-500 hover:bg-green-600 text-white font-semibold py-2 px-4 rounded transition-colors"
            onClick={async () => {
              try {
                const data = await echo(inputText);
                setEchoResponse(data ?? "");
              } catch (e: unknown) {
                const msg = e instanceof Error ? e.message : String(e);
                setEchoResponse(`Request failed: ${msg}`);
              }
            }}
          >
            Send to /api/echo
          </button>

          <label
            htmlFor="echo-output"
            className="block text-sm font-medium text-gray-700"
          >
            Response
          </label>
          <textarea
            id="echo-output"
            className="w-full border rounded px-3 py-2 h-24 resize-y focus:outline-none focus:ring-2 focus:ring-blue-500"
            readOnly
            value={echoResponse}
          />
        </div>

        <p className="text-gray-700">
          Edit{" "}
          <code className="bg-gray-200 px-1 rounded">src/pages/App.tsx</code>{" "}
          and save to test HMR
        </p>
      </div>
    </div>
  );
}
  */

export default Home;
