import { useState } from "react";
import { useNavigate } from "react-router";
import { useLobbyStore } from "../stores/lobbyStore";
import {
  AuthenticatedTemplate,
  UnauthenticatedTemplate,
} from "@azure/msal-react";
import { api } from "../api/client";
import { useIdentity } from "../hooks/useIdentity";
import ParchmentCard from "../components/ui/ParchmentCard";
import CatanButton from "../components/ui/CatanButton";
import CatanAvatar from "../components/ui/CatanAvatar";
import CatanColorPicker from "../components/ui/CatanColorPicker";
import CatanNameEdit from "../components/ui/CatanNameEdit";

function HeroBanner({
  onCreateLobby,
  creating,
  joinCode,
  onJoinCodeChange,
  onJoin,
}: {
  onCreateLobby: () => void;
  creating: boolean;
  joinCode: string;
  onJoinCodeChange: (v: string) => void;
  onJoin: () => void;
}) {
  return (
    <div
      className="rounded-2xl cursor-default border-2 border-(--ink) px-6 py-6 shadow-[4px_5px_0_var(--ink)]"
      style={{ background: "var(--catan-accent)", color: "#fff6df" }}
    >
      <div
        style={{
          fontFamily: "var(--font-serif)",
          fontSize: "2.4rem",
          lineHeight: 1,
        }}
      >
        Settlers of Crutan
      </div>
      <div
        style={{
          fontFamily: "var(--font-hand)",
          fontSize: "1.3rem",
          marginTop: 6,
          opacity: 0.9,
        }}
      >
        Trade. Build. Outwit thy friends.
      </div>

      <div className="mt-5 flex flex-wrap items-center gap-4">
        <CatanButton
          onClick={onCreateLobby}
          disabled={creating}
          style={{
            background: "#f2e7cf",
            color: "var(--ink)",
            fontSize: "1.15rem",
            padding: "10px 20px",
          }}
        >
          {creating ? "Creating…" : "🏰 Create Lobby"}
        </CatanButton>

        <div className="flex items-center gap-2">
          <input
            value={joinCode}
            onChange={(e) => onJoinCodeChange(e.target.value.trim())}
            placeholder="LOBBY CODE"
            className="w-44 rounded-xl border-2 border-[#fff6df] bg-white/20 px-3 py-2 text-sm text-[#fff6df] placeholder:text-[#fff6df]/60 outline-none focus:border-white tracking-widest"
            style={{ fontFamily: "var(--font-mono)" }}
          />
          <button
            type="button"
            disabled={!joinCode}
            onClick={onJoin}
            className="rounded-xl border-2 border-[#fff6df] bg-white/15 px-4 py-2 text-sm font-medium text-[#fff6df] transition-colors hover:bg-white/25 disabled:opacity-40"
            style={{ fontFamily: "var(--font-hand)", fontSize: "1.1rem" }}
          >
            Join →
          </button>
        </div>
      </div>
    </div>
  );
}

function IdentitySheet({
  name,
  color,
  onNameChange,
  onColorChange,
}: {
  name: string;
  color: string;
  onNameChange: (n: string) => void;
  onColorChange: (c: string) => void;
}) {
  return (
    <ParchmentCard tape style={{ flex: 2, minWidth: 320 }}>
      <div
        style={{
          fontFamily: "var(--font-serif)",
          fontSize: "1.5rem",
          marginBottom: 4,
        }}
      >
        Your Banner
      </div>
      <div
        style={{
          fontFamily: "var(--font-hand)",
          color: "var(--ink-soft)",
          marginBottom: 16,
        }}
      >
        This is how others shall know thee.
      </div>
      <div className="flex flex-wrap items-center gap-4">
        <CatanAvatar color={color} name={name} size="lg" />
        <div className="flex flex-col gap-1">
          <div
            style={{
              fontFamily: "var(--font-mono)",
              fontSize: "0.7rem",
              letterSpacing: "0.15em",
              color: "var(--ink-faint)",
            }}
          >
            DISPLAY NAME
          </div>
          <CatanNameEdit
            value={name}
            onChange={onNameChange}
            placeholder="Sir Claude"
          />
        </div>
        <div className="flex flex-col gap-1">
          <div
            style={{
              fontFamily: "var(--font-mono)",
              fontSize: "0.7rem",
              letterSpacing: "0.15em",
              color: "var(--ink-faint)",
            }}
          >
            HOUSE COLOR
          </div>
          <CatanColorPicker value={color} onChange={onColorChange} />
        </div>
      </div>
    </ParchmentCard>
  );
}

function LastGameCard() {
  return (
    <ParchmentCard style={{ flex: 1, minWidth: 220 }}>
      <div
        style={{
          fontFamily: "var(--font-serif)",
          fontSize: "1.35rem",
          marginBottom: 8,
        }}
      >
        Last Game
      </div>
      <div className="flex items-center gap-3">
        <span
          style={{
            fontFamily: "var(--font-serif)",
            fontSize: "2.2rem",
            color: "var(--ink)",
          }}
        >
          —
        </span>
        <span
          style={{ color: "var(--ink-faint)", fontFamily: "var(--font-hand)" }}
        >
          No recent games
        </span>
      </div>
      <div
        className="my-3 h-0.5"
        style={{
          background:
            "repeating-linear-gradient(90deg, var(--ink-soft) 0 6px, transparent 6px 10px)",
        }}
      />
      <CatanButton size="sm" variant="ghost">
        view history →
      </CatanButton>
    </ParchmentCard>
  );
}

function HowToPlayCard() {
  return (
    <ParchmentCard>
      <div className="flex items-center justify-between flex-wrap gap-4">
        <div>
          <div style={{ fontFamily: "var(--font-serif)", fontSize: "1.35rem" }}>
            How to play
          </div>
          <div
            style={{ fontFamily: "var(--font-hand)", color: "var(--ink-soft)" }}
          >
            Sixty-second rules refresher.
          </div>
        </div>
        <CatanButton size="sm" variant="ghost">
          Open guide 📖
        </CatanButton>
      </div>
    </ParchmentCard>
  );
}

function HomePage() {
  const [lobbyCode, setLobbyCode] = useState<string>("");
  const navigate = useNavigate();
  const status = useLobbyStore((s) => s.status);
  const error = useLobbyStore((s) => s.error);
  const { name, color, setName, setColor } = useIdentity();

  const handleCreateLobby = async () => {
    const { data } = await api.POST("/api/lobby/create");
    if (data) navigate(`/lobby/${data}`);
  };

  const handleJoin = () => {
    if (!lobbyCode) return;
    navigate(`/lobby/${lobbyCode}`);
  };

  return (
    <div className="flex flex-col gap-6 py-4">
      <AuthenticatedTemplate>
        <HeroBanner
          onCreateLobby={() => void handleCreateLobby()}
          creating={status === "loading"}
          joinCode={lobbyCode}
          onJoinCodeChange={setLobbyCode}
          onJoin={handleJoin}
        />

        <div className="flex flex-wrap gap-5">
          <IdentitySheet
            name={name}
            color={color}
            onNameChange={setName}
            onColorChange={setColor}
          />
          <LastGameCard />
        </div>

        <HowToPlayCard />
      </AuthenticatedTemplate>

      <UnauthenticatedTemplate>
        <div
          className="rounded-2xl border-2 border-(--ink) px-6 py-6 shadow-[4px_5px_0_var(--ink)]"
          style={{ background: "var(--catan-accent)", color: "#fff6df" }}
        >
          <div
            style={{
              fontFamily: "var(--font-serif)",
              fontSize: "2.4rem",
              lineHeight: 1,
            }}
          >
            Settlers of Crutan
          </div>
          <div
            style={{
              fontFamily: "var(--font-hand)",
              fontSize: "1.3rem",
              marginTop: 6,
              opacity: 0.9,
            }}
          >
            Trade. Build. Outwit thy friends.
          </div>
          <p
            className="mt-5 inline-block rounded-xl border-2 border-[#fff6df] bg-white/15 px-4 py-2 text-sm"
            style={{ fontFamily: "var(--font-hand)", fontSize: "1.05rem" }}
          >
            Sign in to create or join a lobby.
          </p>
        </div>
      </UnauthenticatedTemplate>

      {error && (
        <p
          className="rounded-xl border-2 border-(--catan-accent) bg-red-50 px-4 py-3 text-center text-sm"
          style={{ color: "var(--catan-accent)" }}
          role="alert"
        >
          {error}
        </p>
      )}
    </div>
  );
}

export default HomePage;
