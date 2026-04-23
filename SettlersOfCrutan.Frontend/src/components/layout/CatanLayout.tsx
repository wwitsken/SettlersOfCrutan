import { useState } from "react";
import type { ReactNode } from "react";
import type { Player } from "../../domain/game/player";
import type { DevelopmentCardType } from "../../domain/game/gameTypes";
import type { ResourceCardType } from "../../domain/game/gameTypes";
import type { ChatMessage } from "../../domain/game/gameTypes";
import {
  DEV_TYPES_PLAYED_STRIP,
  RESOURCE_HAND_TYPES,
} from "../../constants/catanMeta";
import PlayerRow from "../game/PlayerRow";
import ResourceCard from "../game/ResourceCard";
import UnplayedDevCardTile, {
  type UnplayedDevCardView,
} from "../game/UnplayedDevCardTile";
import PlayedDevCardTile from "../game/PlayedDevCardTile";
import ChatStub from "../ui/ChatStub";

interface CatanLayoutProps {
  players: (Player & { isCurrentTurn: boolean })[];
  chatMessages: ChatMessage[];
  onSendChatMessage?: (text: string) => void;
  chatDisabled?: boolean;
  resourceHand: Record<string, number>;
  unplayedDevCards: UnplayedDevCardView[];
  playedDevCards: Record<DevelopmentCardType, number>;
  currentPlayerName: string;
  currentPlayerColor: Player["playerColor"];
  boardSlot: ReactNode;
  actionBarSlot: ReactNode;
  resourceMaritimeEnabled: boolean;
  onResourceCardMaritime?: (
    resource: ResourceCardType,
    anchorEl: HTMLButtonElement,
  ) => void;
  unplayedDevPlayEnabled: boolean;
  onUnplayedDevCardClick?: (type: DevelopmentCardType) => void;
}

// ── Section label ────────────────────────────────────────────────────────────
function SectionLabel({ children }: { children: ReactNode }) {
  return (
    <div
      className="mb-2 px-1"
      style={{
        fontFamily: "var(--font-mono)",
        fontSize: "0.65rem",
        letterSpacing: "0.15em",
        color: "var(--ink-faint)",
        textTransform: "uppercase",
      }}
    >
      {children}
    </div>
  );
}

// ── Parchment panel ──────────────────────────────────────────────────────────
function Panel({
  children,
  className = "",
}: {
  children: ReactNode;
  className?: string;
}) {
  return (
    <div
      className={`cursor-default rounded-2xl border-2 border-(--ink) bg-(--parchment) p-3 shadow-[3px_3px_0_var(--ink)] ${className}`}
    >
      {children}
    </div>
  );
}

// ── Honors strip ─────────────────────────────────────────────────────────────
function HonorsPanel({
  players,
}: {
  players: (Player & { isCurrentTurn: boolean })[];
}) {
  const longestRoadHolder = players.find((p) => p.hasLongestRoad)?.displayName;
  const largestArmyHolder = players.find((p) => p.hasLargestArmy)?.displayName;

  return (
    <Panel>
      <SectionLabel>Honors</SectionLabel>
      <div className="flex flex-col gap-1.5">
        <HonorTag label="longest road" holder={longestRoadHolder} />
        <HonorTag label="largest army" holder={largestArmyHolder} />
      </div>
    </Panel>
  );
}

function HonorTag({ label, holder }: { label: string; holder?: string }) {
  return (
    <div
      className="inline-flex items-center gap-1 rounded-full border border-(--ink-soft) bg-white/30 px-2 py-0.5 text-xs"
      style={{
        fontFamily: "var(--font-mono)",
        letterSpacing: "0.05em",
        color: "var(--ink-soft)",
      }}
    >
      {label}
      {holder ? (
        <span
          style={{
            color: "var(--catan-accent)",
            fontWeight: 700,
            marginLeft: 4,
          }}
        >
          · {holder}
        </span>
      ) : (
        <span style={{ color: "var(--ink-faint)", marginLeft: 4 }}>· —</span>
      )}
    </div>
  );
}

// ── Main layout ───────────────────────────────────────────────────────────────
export default function CatanLayout({
  players,
  chatMessages,
  onSendChatMessage,
  chatDisabled = false,
  resourceHand,
  unplayedDevCards,
  playedDevCards,
  currentPlayerName,
  currentPlayerColor: _currentPlayerColor,
  boardSlot,
  actionBarSlot,
  resourceMaritimeEnabled,
  onResourceCardMaritime,
  unplayedDevPlayEnabled,
  onUnplayedDevCardClick,
}: CatanLayoutProps) {
  const [chatOpen, setChatOpen] = useState(true);

  const currentTurnPlayer = players.find((p) => p.isCurrentTurn);

  return (
    <div
      className="min-h-screen flex flex-col gap-3 p-3"
      style={{ background: "var(--parchment)", fontFamily: "var(--font-hand)" }}
    >
      {/* ── TOP: Left Action Tent + Center Board + Right Chat ── */}
      <div
        className="flex gap-3 flex-1 min-h-0"
        style={{ height: "calc(100vh - 260px)" }}
      >
        {/* LEFT — Action Tent (w-56) */}
        <div className="flex flex-col gap-3 w-56 shrink-0">
          {/* Turn + action panel */}
          <Panel className="flex flex-col gap-2">
            <div>
              <div
                style={{
                  fontFamily: "var(--font-serif)",
                  fontSize: "1rem",
                  color: "var(--ink)",
                }}
              >
                Turn:{" "}
                <span style={{ color: "var(--catan-accent)" }}>
                  {currentTurnPlayer?.displayName ?? "—"}
                </span>
              </div>
            </div>
            {actionBarSlot}
          </Panel>

          {/* Honors */}
          <HonorsPanel players={players} />
        </div>

        {/* CENTER — Player strip + Board */}
        <div className="flex flex-col gap-3 flex-1 min-w-0">
          {/* Player strip */}
          <div className="flex gap-2 flex-wrap shrink-0">
            {players.map((p) => (
              <div key={p.id} className="flex-1 min-w-36">
                <PlayerRow player={p} isCurrentTurn={p.isCurrentTurn} />
              </div>
            ))}
            {players.length === 0 && (
              <span
                className="text-sm italic"
                style={{
                  color: "var(--ink-faint)",
                  fontFamily: "var(--font-hand)",
                }}
              >
                No players loaded
              </span>
            )}
          </div>

          {/* Board */}
          <div
            className="flex-1 min-h-0 rounded-2xl border-2 border-(--ink) overflow-hidden flex flex-col shadow-[3px_3px_0_var(--ink)]"
            style={{ background: "var(--parchment-3)" }}
          >
            {boardSlot}
          </div>
        </div>

        {/* RIGHT — Chat (collapsible) */}
        <div
          className="shrink-0 flex flex-col transition-all"
          style={{ width: chatOpen ? 280 : 48 }}
        >
          {chatOpen ? (
            <ChatStub
              title="Tavern Chat"
              messages={chatMessages}
              onSend={onSendChatMessage}
              disabled={chatDisabled}
              onToggleCollapsed={() => setChatOpen(false)}
              className="h-full"
            />
          ) : (
            <button
              type="button"
              onClick={() => setChatOpen(true)}
              className="cursor-pointer h-40 w-12 rounded-2xl border-2 border-(--ink) bg-(--parchment-2) shadow-[2px_2px_0_var(--ink)] transition-transform hover:-translate-x-px"
              style={{
                writingMode: "vertical-rl",
                transform: "rotate(180deg)",
                fontFamily: "var(--font-hand)",
                fontSize: "1rem",
                color: "var(--ink-soft)",
                padding: "10px 4px",
              }}
            >
              ▸ Chat
            </button>
          )}
        </div>
      </div>

      {/* ── BOTTOM — My Stash ── */}
      <Panel>
        <div
          className="mb-2 flex items-center justify-between"
          style={{
            fontFamily: "var(--font-serif)",
            fontSize: "1.05rem",
            color: "var(--ink)",
          }}
        >
          My Stash
          <span
            className="rounded-full border border-(--ink-soft) bg-white/30 px-2 py-0.5 text-xs"
            style={{
              fontFamily: "var(--font-mono)",
              color: "var(--ink-faint)",
              letterSpacing: "0.1em",
            }}
          >
            {currentPlayerName || "private"}
          </span>
        </div>

        <div className="flex gap-6 flex-wrap items-start">
          {/* Resources */}
          <div>
            <SectionLabel>Resources</SectionLabel>
            <div className="flex gap-2 flex-wrap">
              {RESOURCE_HAND_TYPES.map((r) => (
                <ResourceCard
                  key={r}
                  type={r}
                  count={resourceHand[r] ?? 0}
                  interactive={
                    resourceMaritimeEnabled && !!onResourceCardMaritime
                  }
                  disabled={!resourceMaritimeEnabled || !onResourceCardMaritime}
                  onClick={
                    onResourceCardMaritime && resourceMaritimeEnabled
                      ? (anchorEl) => onResourceCardMaritime(r, anchorEl)
                      : undefined
                  }
                />
              ))}
            </div>
          </div>

          {/* Unplayed Dev Cards */}
          <div>
            <SectionLabel>Dev Cards (unplayed)</SectionLabel>
            <div className="flex gap-2 flex-wrap">
              {unplayedDevCards.map((card) => (
                <UnplayedDevCardTile
                  key={card.id}
                  card={card}
                  disabled={!unplayedDevPlayEnabled || !onUnplayedDevCardClick}
                  onClick={
                    onUnplayedDevCardClick && unplayedDevPlayEnabled
                      ? () => onUnplayedDevCardClick(card.type)
                      : undefined
                  }
                />
              ))}
              {unplayedDevCards.length === 0 && (
                <span
                  className="text-xs italic"
                  style={{
                    color: "var(--ink-faint)",
                    fontFamily: "var(--font-hand)",
                  }}
                >
                  None held
                </span>
              )}
            </div>
          </div>

          {/* Played Dev Cards */}
          <div>
            <SectionLabel>Dev Cards (played)</SectionLabel>
            <div className="flex gap-2 flex-wrap">
              {DEV_TYPES_PLAYED_STRIP.map((t) => (
                <PlayedDevCardTile
                  key={t}
                  type={t}
                  count={playedDevCards[t] ?? 0}
                />
              ))}
              {DEV_TYPES_PLAYED_STRIP.every(
                (t) => (playedDevCards[t] ?? 0) === 0,
              ) && (
                <span
                  className="text-xs italic"
                  style={{
                    color: "var(--ink-faint)",
                    fontFamily: "var(--font-hand)",
                  }}
                >
                  None played yet
                </span>
              )}
            </div>
          </div>
        </div>
      </Panel>
    </div>
  );
}
