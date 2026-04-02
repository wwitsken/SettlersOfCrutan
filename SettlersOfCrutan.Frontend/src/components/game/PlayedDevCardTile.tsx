import type { DevelopmentCardType } from "../../domain/game/gameTypes";
import { DEV_META } from "../../constants/catanMeta";

interface Props {
  type: DevelopmentCardType;
  count: number;
}

export default function PlayedDevCardTile({ type, count }: Props) {
  const meta = DEV_META[type];
  if (count === 0) return null;
  return (
    <div className={`relative rounded-xl border-2 ${meta.border} ${meta.bg} w-14 h-20 flex flex-col items-center justify-center gap-1 opacity-75`}>
      <span className="text-xl">{meta.emoji}</span>
      <span className="text-[10px] text-stone-400 font-medium text-center leading-tight px-1">{meta.label}</span>
      <div className={`absolute -top-2 -right-2 w-6 h-6 rounded-full bg-stone-900 border-2 ${meta.border} flex items-center justify-center`}>
        <span className="text-xs font-bold text-white">{count}</span>
      </div>
    </div>
  );
}
