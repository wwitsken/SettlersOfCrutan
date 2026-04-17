import type { ReactNode } from "react";
import type { Player } from "../../domain/game/player";
import type { DevelopmentCardType } from "../../domain/game/gameTypes";
import type { ResourceCardType } from "../../domain/game/gameTypes";
import type { ChatMessage } from "../../types/catan";
import {
  COLOR_TEXT_MAP,
  DEV_TYPES_PLAYED_STRIP,
  RESOURCE_HAND_TYPES,
} from "../../constants/catanMeta";
import PlayerRow from "../game/PlayerRow";
import ResourceCard from "../game/ResourceCard";
import UnplayedDevCardTile, {
  type UnplayedDevCardView,
} from "../game/UnplayedDevCardTile";
import PlayedDevCardTile from "../game/PlayedDevCardTile";
import ChatPanel from "../game/ChatPanel";

interface CatanLayoutProps {
  players: (Player & { isCurrentTurn: boolean })[];
  chatMessages: ChatMessage[];
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

export default function CatanLayout({
  players,
  chatMessages,
  resourceHand,
  unplayedDevCards,
  playedDevCards,
  currentPlayerName,
  currentPlayerColor,
  boardSlot,
  actionBarSlot,
  resourceMaritimeEnabled,
  onResourceCardMaritime,
  unplayedDevPlayEnabled,
  onUnplayedDevCardClick,
}: CatanLayoutProps) {
  return (
    <div
      className="min-h-screen bg-stone-950 text-stone-100 flex flex-col p-3 gap-3"
      style={{ fontFamily: "'Georgia', 'Palatino', serif" }}
    >
      {/* ── TOP: Board + Sidebar ── */}
      <div
        className="flex gap-3 flex-1 min-h-0"
        style={{ height: "calc(100vh - 280px)" }}
      >
        {/* Board slot */}
        <div className="flex-1 rounded-2xl border border-stone-700/60 bg-stone-900/60 overflow-hidden flex flex-col">
          {boardSlot}
        </div>

        {/* Sidebar */}
        <div className="sm:w-sm lg:w-lg flex flex-col gap-3 shrink-0">
          <div className="rounded-2xl border border-stone-700/60 bg-stone-900/60 p-3 flex flex-col gap-2">
            <div className="text-xs font-semibold text-stone-500 uppercase tracking-widest mb-1 px-1">
              Players
            </div>
            {players.map((p) => (
              <PlayerRow
                key={p.id}
                player={p}
                isCurrentTurn={p.isCurrentTurn}
              />
            ))}
            {players.length === 0 && (
              <span className="text-xs text-stone-700 italic px-1">No players loaded</span>
            )}
          </div>

          <div className="flex-1 rounded-2xl border border-stone-700/60 bg-stone-900/60 p-3 overflow-hidden">
            <ChatPanel messages={chatMessages} />
          </div>
        </div>
      </div>

      {/* ── ACTION BAR ── */}
      <div className="rounded-2xl border border-stone-700/60 bg-stone-900/60 overflow-hidden">
        {actionBarSlot}
      </div>

      {/* ── BOTTOM: Resources · Unplayed Dev Cards · Played Dev Cards ── */}
      <div className="flex gap-3">
        {/* Resources */}
        <div className="flex-1 rounded-2xl border border-stone-700/60 bg-stone-900/60 p-3 flex flex-col">
          <div className="text-xs font-semibold text-stone-500 uppercase tracking-widest mb-3 px-1">
            Resources{currentPlayerName && (
              <>
                {" — "}
                <span className={COLOR_TEXT_MAP[currentPlayerColor]}>
                  {currentPlayerName}
                </span>
              </>
            )}
          </div>
          <div className="flex gap-2 flex-wrap">
            {RESOURCE_HAND_TYPES.map((r) => (
              <ResourceCard
                key={r}
                type={r}
                count={resourceHand[r] ?? 0}
                interactive={resourceMaritimeEnabled && !!onResourceCardMaritime}
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
        <div className="flex-1 rounded-2xl border border-stone-700/60 bg-stone-900/60 p-3 flex flex-col">
          <div className="text-xs font-semibold text-stone-500 uppercase tracking-widest mb-3 px-1">
            Unplayed Dev Cards
          </div>
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
              <span className="text-xs text-stone-700 italic">None held</span>
            )}
          </div>
        </div>

        {/* Played Dev Cards */}
        <div className="flex-1 rounded-2xl border border-stone-700/60 bg-stone-900/60 p-3 flex flex-col">
          <div className="text-xs font-semibold text-stone-500 uppercase tracking-widest mb-3 px-1">
            Played Dev Cards
          </div>
          <div className="flex gap-2 flex-wrap">
            {DEV_TYPES_PLAYED_STRIP.map((t) => (
              <PlayedDevCardTile key={t} type={t} count={playedDevCards[t] ?? 0} />
            ))}
            {DEV_TYPES_PLAYED_STRIP.every((t) => (playedDevCards[t] ?? 0) === 0) && (
              <span className="text-xs text-stone-700 italic">None played yet</span>
            )}
          </div>
        </div>
      </div>
    </div>
  );
}
