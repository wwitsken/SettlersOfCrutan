import type { UnplayedDevCard } from "../../types/catan";
import { DEV_META } from "../../constants/catanMeta";

export default function UnplayedDevCardTile({ card }: { card: UnplayedDevCard }) {
  const meta = DEV_META[card.type];
  return (
    <div className={`relative rounded-xl border-2 ${meta.border} ${meta.bg} w-14 h-20 flex flex-col items-center justify-center gap-1 cursor-pointer hover:brightness-125 transition-all`}>
      <span className="text-xl">{meta.emoji}</span>
      <span className="text-[10px] text-stone-400 font-medium text-center leading-tight px-1">{meta.label}</span>
    </div>
  );
}
