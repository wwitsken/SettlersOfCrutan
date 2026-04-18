import type { ReactNode, CSSProperties } from "react";

interface ParchmentCardProps {
  children: ReactNode;
  className?: string;
  style?: CSSProperties;
  tape?: boolean;
  padding?: string;
  role?: string;
}

export default function ParchmentCard({
  children,
  className = "",
  style,
  tape = false,
  padding = "p-5",
  role,
}: ParchmentCardProps) {
  return (
    <div
      role={role}
      className={`cursor-default relative rounded-2xl border-2 border-(--ink) bg-(--parchment) shadow-[4px_5px_0_var(--ink)] ${padding} ${className}`}
      style={style}
    >
      {tape && (
        <div
          className="absolute left-8 -top-3 h-5 w-20 -rotate-[4deg] border border-dashed border-(--ink-soft) opacity-70"
          style={{ background: "rgba(210,180,110,0.7)" }}
        />
      )}
      {children}
      {/* corner fold */}
      <div
        className="pointer-events-none absolute right-0 bottom-0 h-6 w-6 border-t-2 border-l-2 border-(--ink) bg-(--parchment-3)"
        style={{ clipPath: "polygon(100% 0, 100% 100%, 0 100%)" }}
      />
    </div>
  );
}
