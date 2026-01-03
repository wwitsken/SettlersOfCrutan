import { useState } from "react";
import { useNavigate } from "react-router";
import { useLobbyStore } from "../stores/lobbyStore";
import {
  AuthenticatedTemplate,
  UnauthenticatedTemplate,
} from "@azure/msal-react";
import { api } from "../api/client";

function HomePage() {
  const [lobby, setLobby] = useState<string>("");
  const navigate = useNavigate();
  const status = useLobbyStore((s) => s.status);
  const error = useLobbyStore((s) => s.error);
  return (
    <div className="bg-gray-100 flex flex-col items-center justify-center">
      <h1 className="text-4xl font-bold text-blue-600 mb-6">
        Settlers of Crutan
      </h1>
      <AuthenticatedTemplate>
        <button
          onClick={async () => {
            const { data } = await api.POST("/api/lobby/create");
            if (data) navigate(`/lobby/${data}`);
          }}
          className="mb-4 px-3 py-2 border rounded bg-white hover:bg-gray-50 cursor-pointer"
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
              // await joinLobby(lobby);
              navigate(`/lobby/${lobby}`);
            }}
            className="p-1 border rounded-sm border-gray-600 bg-white hover:bg-gray-50 cursor-pointer"
            disabled={!lobby || status === "loading"}
          >
            {status === "loading" ? "Joining…" : "Join lobby"}
          </button>
        </div>
      </AuthenticatedTemplate>
      <UnauthenticatedTemplate>
        <p>You gotta log in, bud</p>
      </UnauthenticatedTemplate>
      {error && <p className="mt-2 text-sm text-red-600">{error}</p>}
    </div>
  );
}

export default HomePage;
