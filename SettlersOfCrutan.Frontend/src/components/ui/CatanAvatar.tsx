const COLOR_BG: Record<string, string> = {
  red:    "#c94a3a",
  blue:   "#3b6ea8",
  white:  "#f3ecd6",
  orange: "#e49246",
  green:  "#6b8e4e",
  yellow: "#c9a94a",
  brown:  "#78350f",
  purple: "#7c3aed",
  none:   "#94a3b8",
};

interface CatanAvatarProps {
  color: string;
  name: string;
  size?: "sm" | "md" | "lg";
  host?: boolean;
}

const SIZE = {
  sm: "w-6 h-6 text-xs",
  md: "w-8 h-8 text-base",
  lg: "w-11 h-11 text-xl",
};

export default function CatanAvatar({ color, name, size = "md", host = false }: CatanAvatarProps) {
  const initial = (name || "?").trim().charAt(0).toUpperCase();
  const bg = COLOR_BG[color] ?? COLOR_BG.none;
  const isLight = color === "white";

  return (
    <span className="relative inline-flex flex-shrink-0 items-center justify-center">
      <span
        className={`inline-flex items-center justify-center rounded-full border-2 border-[var(--ink)] shadow-[2px_2px_0_var(--ink)] font-[var(--font-serif)] ${SIZE[size]}`}
        style={{
          background: bg,
          color: isLight ? "var(--ink)" : "#fff7e3",
        }}
        title={name}
      >
        {initial}
      </span>
      {host && (
        <span
          className="absolute -top-2.5 -right-1.5 text-sm leading-none"
          style={{ color: "#c9a94a", textShadow: "1px 1px 0 var(--ink)" }}
          title="Host"
        >
          ♛
        </span>
      )}
    </span>
  );
}
