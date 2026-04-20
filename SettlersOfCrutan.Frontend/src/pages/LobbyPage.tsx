import { useEffect, useState } from "react";
import {
  useLoaderData,
  useNavigate,
  useParams,
  type LoaderFunctionArgs,
} from "react-router";
import { useIdentity } from "../hooks/useIdentity";
import { useLobbyStore } from "../stores/lobbyStore";
import { api } from "../api/client";
import { getAccessTokenForOpenApi } from "../authConfig";
import { useSignalRContext } from "../context/SignalRContext";
import { lobbyDtoToDomain } from "../domain/lobby/mapLobbyDto";
import { lobbyFullStateEvents } from "../api/realtimeEvents";
import { fetchUserProfiles, type UserProfile } from "../api/userProfiles";
import type { components } from "../api/types";
import ParchmentCard from "../components/ui/ParchmentCard";
import CatanButton from "../components/ui/CatanButton";
import CatanAvatar from "../components/ui/CatanAvatar";
import CatanColorPicker from "../components/ui/CatanColorPicker";
import CatanNameEdit from "../components/ui/CatanNameEdit";
import ReadyToggle from "../components/ui/ReadyToggle";
import CodeChip from "../components/ui/CodeChip";
import ChatStub from "../components/ui/ChatStub";
import { useUserProfiles } from "../hooks/useUserName";

// ── Loader ──────────────────────────────────────────────────────

type LobbyLoaderResult = {
  loadedStatus: number;
  lobbyData: components["schemas"]["LobbyDto"] | null;
  users: UserProfile[];
};

export async function LobbyLoader(
  args: LoaderFunctionArgs,
): Promise<LobbyLoaderResult> {
  if (!args.params.lobbyId)
    return { loadedStatus: 404, lobbyData: null, users: [] };

  const { data: lobbyData, response: lobbyResponse } = await api.GET(
    "/api/lobby/{lobbyId}",
    {
      params: { path: { lobbyId: args.params.lobbyId } },
      accessToken: (await getAccessTokenForOpenApi()) ?? "",
    },
  );

  if (lobbyResponse.status !== 200 || !lobbyData) {
    return { loadedStatus: lobbyResponse.status, lobbyData: null, users: [] };
  }

  const userIds = (lobbyData.lobbyMembers ?? [])
    .map((m) => m.userId)
    .filter((id): id is string => typeof id === "string" && id.length > 0);
  const users = await fetchUserProfiles(userIds);

  return { loadedStatus: 200, lobbyData, users };
}

// ── Sub-components ──────────────────────────────────────────────────────────

interface PlayerSlotProps {
  displayName?: string;
  preferredColor?: string;
  isMe: boolean;
  isHost: boolean;
  isReady: boolean;
  myName: string;
  myColor: string;
  takenColors: string[];
  onNameChange: (n: string) => void;
  onColorChange: (c: string) => void;
  onReadyToggle: () => void;
}

function PlayerSlotRow({
  displayName,
  preferredColor,
  isMe,
  isHost,
  isReady,
  myName,
  myColor,
  takenColors,
  onNameChange,
  onColorChange,
  onReadyToggle,
}: PlayerSlotProps) {
  const shownName = isMe ? myName : (displayName ?? "Player");
  const shownColor = isMe ? myColor : (preferredColor ?? "none");

  return (
    <div className="flex items-center gap-3 rounded-xl border-2 border-(--ink) bg-(--parchment-2) px-3 py-2.5 shadow-[2px_2px_0_var(--ink)] flex-wrap">
      <CatanAvatar
        color={shownColor}
        name={shownName}
        host={isHost}
        size="md"
      />

      <div className="flex flex-1 min-w-0 items-center gap-3 flex-wrap">
        {isMe ? (
          <>
            <CatanNameEdit value={myName} onChange={onNameChange} />
            <CatanColorPicker
              value={myColor}
              onChange={onColorChange}
              taken={takenColors}
            />
          </>
        ) : (
          <span style={{ fontFamily: "var(--font-serif)", fontSize: "1.1rem" }}>
            {shownName}
            {isHost && (
              <span
                style={{
                  marginLeft: 8,
                  fontSize: "0.85rem",
                  color: "var(--ink-faint)",
                }}
              >
                host
              </span>
            )}
          </span>
        )}
      </div>

      {isMe ? (
        <ReadyToggle
          on={isReady}
          onChange={() => {
            onReadyToggle();
          }}
        />
      ) : (
        <span
          className="whitespace-nowrap rounded-full border-2 border-(--ink) px-3 py-0.5 text-sm"
          style={{
            fontFamily: "var(--font-hand)",
            background: isReady ? "#c9d9a8" : "var(--parchment)",
            color: isReady ? "var(--ink)" : "var(--ink-faint)",
          }}
        >
          {isReady ? "ready" : "not ready"}
        </span>
      )}
    </div>
  );
}

function EmptySeat({ onInvite }: { onInvite: () => void }) {
  return (
    <div className="flex items-center gap-3 rounded-xl border-2 border-dashed border-(--ink-soft) px-3 py-2.5">
      <span
        className="h-8 w-8 rounded-full border-2 border-dashed border-(--ink-soft) shrink-0"
        style={{ background: "transparent" }}
      />
      <span
        className="flex-1 italic"
        style={{ color: "var(--ink-faint)", fontFamily: "var(--font-hand)" }}
      >
        — empty seat —
      </span>
      <CatanButton size="sm" variant="ghost" onClick={onInvite}>
        invite 📜
      </CatanButton>
    </div>
  );
}

// ── Page ────────────────────────────────────────────────────────────────────

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
  const {
    lobbyData,
    loadedStatus,
    users: initialUsers,
  } = useLoaderData<typeof LobbyLoader>();

  const status = useLobbyStore((s) => s.status);
  const error = useLobbyStore((s) => s.error);
  const currentLobby = useLobbyStore((s) => s.currentLobby);
  const setLobby = useLobbyStore((s) => s.setLobby);
  const clearLobby = useLobbyStore((s) => s.clear);

  const { users, fetchProfiles } = useUserProfiles(initialUsers);
  const [latestLobbyDto, setLatestLobbyDto] = useState<
    components["schemas"]["LobbyDto"] | null
  >(lobbyData ?? null);

  const { name, color, setName, setColor } = useIdentity();

  // Re-map lobby to domain whenever raw DTO or fetched profiles change.
  useEffect(() => {
    if (!latestLobbyDto) return;
    const mapped = lobbyDtoToDomain(latestLobbyDto, users);
    if (mapped) setLobby(mapped);
  }, [latestLobbyDto, users, setLobby]);

  // Initial seed came from the route loader; if the user lands here with a
  // non-200 loader status, there is nothing to project.
  useEffect(() => {
    if (loadedStatus !== 200 || !lobbyData) return;
    setLatestLobbyDto(lobbyData);
  }, [loadedStatus, lobbyData]);

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
          if (import.meta.env.DEV)
            console.debug(
              "[SignalR] LobbyReceive ignored (unknown event)",
              eventName,
            );
          return;
        }
        try {
          const dto = (payload ?? {}) as components["schemas"]["LobbyDto"];
          setLatestLobbyDto(dto);
          const userIds = (dto.lobbyMembers ?? [])
            .map((m) => m.userId)
            .filter(Boolean) as string[];
          void fetchProfiles(userIds);
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
  }, [registerHandlers, start, setLobby, clearLobby, navigate, fetchProfiles]);

  const me = currentLobby?.lobbyMembers.find((m) => m.isMe);
  const isInLobby = !!me;
  const isReady = !!me?.isReady;
  const activeLobbyId = currentLobby?.lobbyId ?? lobbyId ?? "";
  const allReady =
    !!currentLobby?.lobbyMembers.length &&
    currentLobby.lobbyMembers.every((m) => m.isReady);

  // Colors taken by other players (visual only until backend wires color assignment)
  const takenColors =
    currentLobby?.lobbyMembers.filter((m) => !m.isMe).map(() => "none") ?? [];

  const handleReadyToggle = async () => {
    if (!lobbyId) return;
    if (isReady) {
      await api.POST("/api/lobby/{lobbyId}/unready", {
        params: { path: { lobbyId } },
      });
    } else {
      await api.POST("/api/lobby/{lobbyId}/ready", {
        params: { path: { lobbyId } },
      });
    }
  };

  // ── Connection states ────────────────────────────────────────────────────

  if (isConnecting && !isConnected) {
    return (
      <ParchmentCard
        className="text-center"
        style={{ color: "var(--ink-soft)" }}
      >
        <span style={{ fontFamily: "var(--font-hand)", fontSize: "1.2rem" }}>
          Connecting to game services…
        </span>
      </ParchmentCard>
    );
  }

  if (signalRError && !isConnected) {
    return (
      <ParchmentCard className="border-(--catan-accent)" role="alert">
        <span
          style={{
            fontFamily: "var(--font-hand)",
            fontSize: "1.1rem",
            color: "var(--catan-accent)",
          }}
        >
          Could not connect to game services: {signalRError}
        </span>
      </ParchmentCard>
    );
  }

  // ── Main UI ───────────────────────────────────────────────────────────────

  const hostName =
    currentLobby?.lobbyMembers.find((m) => m.isHost)?.displayName ?? "Host";

  const MAX_PLAYERS = 4;
  const seatCount = currentLobby?.lobbyMembers.length ?? 0;
  const emptySeats = Math.max(0, MAX_PLAYERS - seatCount);

  return (
    <div className="flex flex-col gap-5 py-4">
      <ParchmentCard>
        {/* Header row */}
        <div className="flex flex-wrap items-start gap-3">
          <div className="flex-1 min-w-0">
            <div
              style={{ fontFamily: "var(--font-serif)", fontSize: "1.6rem" }}
            >
              {hostName}'s Merry Hall
            </div>
            <div
              style={{
                fontFamily: "var(--font-hand)",
                color: "var(--ink-soft)",
                marginTop: 2,
              }}
            >
              Waiting for the fellowship · {seatCount}/{MAX_PLAYERS}
            </div>
          </div>
          {activeLobbyId && <CodeChip code={activeLobbyId} />}
        </div>

        {/* Divider */}
        <div
          className="my-4 h-0.5"
          style={{
            background:
              "repeating-linear-gradient(90deg, var(--ink-soft) 0 6px, transparent 6px 10px)",
          }}
        />

        <div className="flex flex-wrap items-start gap-5">
          {/* Players column */}
          <div className="flex flex-col gap-2" style={{ flex: "2 1 340px" }}>
            <div
              className="mb-1"
              style={{
                fontFamily: "var(--font-mono)",
                fontSize: "0.7rem",
                letterSpacing: "0.15em",
                color: "var(--ink-faint)",
              }}
            >
              PLAYERS ·{" "}
              {currentLobby?.lobbyMembers.filter((m) => m.isReady).length ?? 0}/
              {seatCount} READY
            </div>

            {currentLobby?.lobbyMembers.map((p) => (
              <PlayerSlotRow
                key={p.id}
                displayName={p.displayName}
                preferredColor={p.preferredColor}
                isMe={p.isMe}
                isHost={p.isHost}
                isReady={p.isReady}
                myName={name}
                myColor={color}
                takenColors={takenColors}
                onNameChange={setName}
                onColorChange={setColor}
                onReadyToggle={() => void handleReadyToggle()}
              />
            ))}

            {Array.from({ length: emptySeats }).map((_, i) => (
              <EmptySeat
                key={i}
                onInvite={() =>
                  void navigator.clipboard?.writeText(activeLobbyId)
                }
              />
            ))}

            {!isInLobby && lobbyId && (
              <CatanButton
                onClick={async () =>
                  await api.POST("/api/lobby/{lobbyId}/join", {
                    params: { path: { lobbyId } },
                  })
                }
                disabled={status === "loading"}
              >
                {status === "loading" ? "Joining…" : "Join lobby"}
              </CatanButton>
            )}
          </div>

          {/* Actions + chat column */}
          <div className="flex flex-col gap-4" style={{ flex: "1 1 260px" }}>
            {/* Start quest card */}
            <ParchmentCard padding="p-4">
              <div
                style={{ fontFamily: "var(--font-serif)", fontSize: "1.25rem" }}
              >
                Start the Quest
              </div>
              <div
                style={{
                  fontFamily: "var(--font-hand)",
                  color: "var(--ink-soft)",
                  margin: "4px 0 12px",
                }}
              >
                {allReady
                  ? "All souls ready. Onward!"
                  : "Wait for all to ready up."}
              </div>
              <CatanButton
                variant="primary"
                className="w-full justify-center text-lg"
                disabled={!allReady || status === "loading"}
                onClick={async () => {
                  if (!lobbyId || !allReady) return;
                  await api.POST("/api/lobby/{lobbyId}/start-game", {
                    params: { path: { lobbyId } },
                    body: { gameName: "Crutan Game", gameType: "baseGame" },
                  });
                }}
              >
                ⚔︎ Start Game
              </CatanButton>

              {isInLobby && lobbyId && (
                <CatanButton
                  size="sm"
                  variant="ghost"
                  className="mt-2 w-full justify-center"
                  onClick={() => {
                    void api.POST("/api/lobby/{lobbyId}/leave", {
                      params: { path: { lobbyId } },
                    });
                    navigate("/");
                  }}
                >
                  Leave lobby
                </CatanButton>
              )}
            </ParchmentCard>

            {/* Pre-game chat stub */}
            {/* STUB: Pass onSend prop wired to SignalR when lobby chat is implemented */}
            <ChatStub
              title="Pre-game Chat"
              initialMessages={[]}
              className="flex-1"
            />
          </div>
        </div>
      </ParchmentCard>

      {error && (
        <p
          className="rounded-xl border-2 border-(--catan-accent) bg-red-50 px-4 py-3 text-sm text-center"
          style={{ color: "var(--catan-accent)" }}
          role="alert"
        >
          {error}
        </p>
      )}
    </div>
  );
}

export default LobbyPage;
