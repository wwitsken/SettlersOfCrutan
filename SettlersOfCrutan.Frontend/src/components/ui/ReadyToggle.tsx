interface ReadyToggleProps {
  on: boolean;
  onChange: (next: boolean) => void;
}

export default function ReadyToggle({ on, onChange }: ReadyToggleProps) {
  return (
    <button
      type="button"
      onClick={() => onChange(!on)}
      className="cursor-pointer min-w-28 whitespace-nowrap rounded-full border-2 border-(--ink) px-3 py-1 text-base shadow-[2px_2px_0_var(--ink)] transition-all hover:-translate-x-px hover:-translate-y-px"
      style={{
        fontFamily: "var(--font-hand)",
        fontSize: "1.05rem",
        background: on ? "#c9d9a8" : "var(--parchment)",
        color: on ? "var(--ink)" : "var(--ink-faint)",
      }}
    >
      {on ? "✓ ready!" : "○ not ready"}
    </button>
  );
}
