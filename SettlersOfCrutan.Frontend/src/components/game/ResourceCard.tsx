import type { ResourceCardType } from "../../domain/game/gameTypes";
import { RESOURCE_META } from "../../constants/catanMeta";

interface Props {
  type: ResourceCardType;
  count: number;
}

export default function ResourceCard({ type, count }: Props) {
  const meta = RESOURCE_META[type];
  return (
    <div className={`relative rounded-xl border-2 ${meta.border} ${meta.bg} w-16 h-20 flex flex-col items-center justify-center gap-1 select-none`}>
      <span className="text-2xl">{meta.emoji}</span>
      <span className="text-xs text-stone-400 font-medium tracking-wide">{meta.label}</span>
      <div className={`absolute -top-2 -right-2 w-6 h-6 rounded-full bg-stone-900 border-2 ${meta.border} flex items-center justify-center`}>
        <span className={`text-xs font-bold ${count === 0 ? "text-stone-600" : "text-white"}`}>{count}</span>
      </div>
    </div>
  );
}
