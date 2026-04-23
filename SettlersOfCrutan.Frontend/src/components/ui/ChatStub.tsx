import { useState, useRef, useEffect } from "react";
import type { ChatMessage } from "../../domain/game/gameTypes";
import CatanButton from "./CatanButton";

// Modes:
//   - Controlled (live): pass `messages` and `onSend`. Parent owns the list.
//   - Uncontrolled (stub): omit `messages` (and optionally `onSend`) and the
//     component manages a local list seeded from `initialMessages`.

const COLOR_STYLE: Record<string, string> = {
  red: "#c94a3a",
  blue: "#3b6ea8",
  orange: "#c97a30",
  green: "#4e7038",
  white: "#7a6a4e",
  yellow: "#c9a94a",
  none: "#7a6a4e",
};

interface ChatStubProps {
  title?: string;
  /** When provided, component renders these messages directly (controlled). */
  messages?: ChatMessage[];
  /** Seed for the uncontrolled fallback only. Ignored when `messages` is set. */
  initialMessages?: ChatMessage[];
  onSend?: (text: string) => void;
  collapsed?: boolean;
  onToggleCollapsed?: () => void;
  disabled?: boolean;
  placeholder?: string;
  className?: string;
}

export default function ChatStub({
  title = "Tavern Chat",
  messages,
  initialMessages = [],
  onSend,
  collapsed = false,
  onToggleCollapsed,
  disabled = false,
  placeholder = "say something…",
  className = "",
}: ChatStubProps) {
  const controlled = messages !== undefined;
  const [localMessages, setLocalMessages] =
    useState<ChatMessage[]>(initialMessages);
  const [input, setInput] = useState("");
  const bodyRef = useRef<HTMLDivElement>(null);

  const displayed = controlled ? messages! : localMessages;

  useEffect(() => {
    if (bodyRef.current) {
      bodyRef.current.scrollTop = bodyRef.current.scrollHeight;
    }
  }, [displayed]);

  const handleSend = (e: React.FormEvent) => {
    e.preventDefault();
    const text = input.trim();
    if (!text || disabled) return;
    if (onSend) {
      onSend(text);
    } else if (!controlled) {
      setLocalMessages((prev) => [
        ...prev,
        { id: Date.now(), player: "You", color: "none", text, time: "" },
      ]);
    }
    setInput("");
  };

  const isLive = controlled && !!onSend;

  return (
    <div
      className={`flex flex-col cursor-default overflow-hidden rounded-2xl border-2 border-(--ink) bg-(--parchment) shadow-[3px_3px_0_var(--ink)] ${className}`}
    >
      {/* header */}
      <div
        className="flex items-center justify-between border-b-2 border-(--ink) bg-(--parchment-2) px-3 py-2"
        style={{ fontFamily: "var(--font-serif)", fontSize: "1.05rem" }}
      >
        <span>
          {title}{" "}
          <span
            className="ml-1 rounded-full border border-(--ink-soft) px-1.5 py-px text-xs tracking-widest"
            style={{ fontFamily: "var(--font-mono)", color: "var(--ink-soft)" }}
          >
            {isLive ? "live" : "stub"}
          </span>
        </span>
        {onToggleCollapsed && (
          <button
            type="button"
            onClick={onToggleCollapsed}
            className="cursor-pointer ml-2 rounded border border-(--ink-soft) bg-transparent px-2 py-0.5 text-sm"
            style={{
              fontFamily: "var(--font-hand)",
              color: "var(--ink-faint)",
            }}
          >
            {collapsed ? "◂ open" : "close ▸"}
          </button>
        )}
      </div>

      {!collapsed && (
        <>
          <div
            ref={bodyRef}
            className="flex-1 overflow-y-auto px-3 py-2 space-y-1.5"
            style={{ maxHeight: 240 }}
          >
            {displayed.map((m) => (
              <div
                key={m.id}
                className="text-sm"
                style={{ fontFamily: "var(--font-hand)" }}
              >
                {m.player === "System" ? (
                  <span
                    style={{ color: "var(--ink-faint)", fontStyle: "italic" }}
                  >
                    {m.text}
                  </span>
                ) : (
                  <>
                    <span
                      className="font-bold"
                      style={{
                        color: COLOR_STYLE[m.color] ?? "var(--ink-soft)",
                      }}
                    >
                      {m.player}:
                    </span>{" "}
                    {m.text}
                    {m.time && (
                      <span
                        className="ml-1 text-xs"
                        style={{
                          color: "var(--ink-faint)",
                          fontFamily: "var(--font-mono)",
                        }}
                      >
                        {m.time}
                      </span>
                    )}
                  </>
                )}
              </div>
            ))}
            {displayed.length === 0 && (
              <p
                className="text-xs italic"
                style={{ color: "var(--ink-faint)" }}
              >
                No messages yet…
              </p>
            )}
          </div>

          <form
            className="flex items-center gap-2 border-t-2 border-dashed border-(--ink-soft) px-3 py-2"
            onSubmit={handleSend}
          >
            <input
              value={input}
              onChange={(e) => setInput(e.target.value)}
              placeholder={disabled ? "connecting…" : placeholder}
              disabled={disabled}
              className="min-w-0 flex-1 rounded-lg border-2 border-(--ink) bg-white/40 px-2 py-1 text-sm outline-none focus:border-(--catan-accent) disabled:opacity-60"
              style={{ fontFamily: "var(--font-mono)", color: "var(--ink)" }}
            />
            <CatanButton size="sm" type="submit" disabled={disabled}>
              send
            </CatanButton>
          </form>
        </>
      )}
    </div>
  );
}
