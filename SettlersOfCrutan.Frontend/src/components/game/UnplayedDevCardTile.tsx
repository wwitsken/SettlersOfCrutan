import type { DevelopmentCardType } from "../../domain/game/gameTypes";
import { DEV_META } from "../../constants/catanMeta";

export type UnplayedDevCardView = { id: string; type: DevelopmentCardType };

type Props = {
  card: UnplayedDevCardView;
  disabled?: boolean;
  onClick?: () => void;
};

export default function UnplayedDevCardTile({
  card,
  disabled = false,
  onClick,
}: Props) {
  const meta = DEV_META[card.type];
  const clickable = !!onClick && !disabled;

  if (clickable) {
    return (
      <button
        type="button"
        className={`relative rounded-xl border-2 ${meta.border} ${meta.bg} w-14 h-20 flex flex-col items-center justify-center gap-1 cursor-pointer hover:brightness-125 transition-all disabled:cursor-not-allowed disabled:opacity-40`}
        disabled={disabled}
        onClick={onClick}
        aria-label={`Play ${meta.label}`}
      >
        <span className="text-xl">{meta.emoji}</span>
        <span className="text-[10px] text-stone-400 font-medium text-center leading-tight px-1">{meta.label}</span>
      </button>
    );
  }

  return (
    <div
      className={`relative rounded-xl border-2 ${meta.border} ${meta.bg} w-14 h-20 flex flex-col items-center justify-center gap-1 ${disabled ? "opacity-40" : ""} select-none`}
    >
      <span className="text-xl">{meta.emoji}</span>
      <span className="text-[10px] text-stone-400 font-medium text-center leading-tight px-1">{meta.label}</span>
    </div>
  );
}
