import { useRef } from "react";
import type { ResourceCardType } from "../../domain/game/gameTypes";
import { RESOURCE_META } from "../../constants/catanMeta";

interface Props {
  type: ResourceCardType;
  count: number;
  interactive?: boolean;
  disabled?: boolean;
  /** Called with the button element for anchored popovers (rect + scroll parents). */
  onClick?: (anchorEl: HTMLButtonElement) => void;
}

export default function ResourceCard({
  type,
  count,
  interactive = false,
  disabled = false,
  onClick,
}: Props) {
  const btnRef = useRef<HTMLButtonElement>(null);
  const meta = RESOURCE_META[type];
  const inner = (
    <>
      <span className="text-2xl">{meta.emoji}</span>
      <span className="text-xs text-stone-400 font-medium tracking-wide">{meta.label}</span>
      <div className={`absolute -top-2 -right-2 w-6 h-6 rounded-full bg-stone-900 border-2 ${meta.border} flex items-center justify-center`}>
        <span className={`text-xs font-bold ${count === 0 ? "text-stone-600" : "text-white"}`}>{count}</span>
      </div>
    </>
  );

  const shell = `relative rounded-xl border-2 ${meta.border} ${meta.bg} w-16 h-20 flex flex-col items-center justify-center gap-1 select-none`;

  if (interactive && onClick) {
    return (
      <button
        ref={btnRef}
        type="button"
        className={`${shell} ${disabled ? "cursor-not-allowed opacity-50" : "cursor-pointer hover:brightness-110 transition-all"}`}
        disabled={disabled}
        onClick={() => {
          if (!btnRef.current) return;
          onClick(btnRef.current);
        }}
        aria-label={`${meta.label}, ${count} in hand. Open maritime trade.`}
      >
        {inner}
      </button>
    );
  }

  return <div className={shell}>{inner}</div>;
}
