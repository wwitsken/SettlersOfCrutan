import { useState } from "react";
import { useNavigate } from "react-router";
import { useLobbyStore } from "../stores/lobbyStore";
import {
  AuthenticatedTemplate,
  UnauthenticatedTemplate,
} from "@azure/msal-react";
import { api } from "../api/client";

function HomePage() {
  const [lobbyCode, setLobbyCode] = useState<string>("");
  const navigate = useNavigate();
  const status = useLobbyStore((s) => s.status);
  const error = useLobbyStore((s) => s.error);

  return (
    <div className="mx-auto flex max-w-lg flex-col gap-8 py-8">
      <div className="text-center">
        <h1 className="text-3xl font-semibold tracking-tight text-slate-900">
          Settlers of Crutan
        </h1>
        <p className="mt-2 text-slate-600">
          Create a lobby or enter a code from the host.
        </p>
      </div>

      <AuthenticatedTemplate>
        <div className="space-y-6 rounded-2xl border border-slate-200 bg-white p-8 shadow-sm">
          <button
            type="button"
            onClick={async () => {
              const { data } = await api.POST("/api/lobby/create");
              if (data) navigate(`/lobby/${data}`);
            }}
            className="w-full rounded-xl bg-slate-900 py-3 text-sm font-medium text-white hover:bg-slate-800 disabled:opacity-50"
            disabled={status === "loading"}
          >
            {status === "loading" ? "Creating…" : "Create lobby"}
          </button>

          <div className="relative">
            <div
              className="absolute inset-0 flex items-center"
              aria-hidden="true"
            >
              <div className="w-full border-t border-slate-200" />
            </div>
            <div className="relative flex justify-center text-xs uppercase">
              <span className="bg-white px-2 text-slate-400">or join</span>
            </div>
          </div>

          <div className="flex gap-2">
            <input
              value={lobbyCode}
              className="min-w-0 flex-1 rounded-xl border border-slate-300 px-3 py-2 text-sm outline-none ring-slate-400 focus:ring-2"
              placeholder="Lobby code (UUID)"
              onChange={(e) => setLobbyCode(e.target.value.trim())}
            />
            <button
              type="button"
              onClick={() => {
                if (!lobbyCode) return;
                navigate(`/lobby/${lobbyCode}`);
              }}
              className="rounded-xl border border-slate-300 bg-white px-4 py-2 text-sm font-medium hover:bg-slate-50 disabled:opacity-50"
              disabled={!lobbyCode || status === "loading"}
            >
              Go
            </button>
          </div>
        </div>
      </AuthenticatedTemplate>

      <UnauthenticatedTemplate>
        <p className="rounded-xl border border-amber-200 bg-amber-50 px-4 py-3 text-center text-sm text-amber-900">
          Sign in to create or join a lobby.
        </p>
      </UnauthenticatedTemplate>

      {error && (
        <p className="text-center text-sm text-red-600" role="alert">
          {error}
        </p>
      )}
    </div>
  );
}

export default HomePage;
