import type { Player } from "../../domain/game/player";
import type { PrivateGameInfo } from "../../domain/game/privateGame";
import type { ResourceCardType } from "../../domain/game/gameTypes";

const RESOURCE_ORDER: ResourceCardType[] = [
  "brick",
  "lumber",
  "wool",
  "grain",
  "ore",
];

const RESOURCE_LABEL: Record<ResourceCardType, string> = {
  none: "—",
  brick: "Brick",
  lumber: "Lumber",
  wool: "Wool",
  grain: "Grain",
  ore: "Ore",
  desert: "Desert",
  water: "Water",
};

const RESOURCE_SWATCH: Record<ResourceCardType, string> = {
  brick: "bg-orange-800",
  lumber: "bg-emerald-700",
  wool: "bg-lime-500",
  grain: "bg-yellow-200",
  ore: "bg-slate-500",
  none: "bg-slate-300",
  desert: "bg-amber-200",
  water: "bg-sky-500",
};

type Props = {
  show: boolean;
  privateGame: PrivateGameInfo | null;
  me: Player | undefined;
};

/**
 * Placeholder “cards” for the current player’s hand and pieces. DOM only, under the canvas.
 */
export function PlayerInventoryStrip({ show, privateGame, me }: Props) {
  if (!show || !privateGame || !me) return null;

  const { resources, devCards, buildables } = privateGame.myHand;

  const devEntries = Object.entries(devCards).filter(([, n]) => n > 0);

  return (
    <div className="shrink-0 border-t border-slate-200 bg-white px-3 py-2">
      <p className="mb-1.5 text-[10px] font-semibold uppercase tracking-wide text-slate-500">
        Your hand
      </p>
      <div className="flex flex-wrap items-end gap-3">
        <div className="flex flex-wrap gap-1">
          {RESOURCE_ORDER.map((rt) => {
            const n = resources[rt] ?? 0;
            return (
              <div
                key={rt}
                className="flex w-11 flex-col items-center gap-0.5 rounded border border-slate-200 bg-slate-50 px-1 py-1"
                title={RESOURCE_LABEL[rt]}
              >
                <div
                  className={`h-7 w-full rounded-sm ${RESOURCE_SWATCH[rt]} shadow-sm`}
                />
                <span className="text-[11px] font-semibold tabular-nums text-slate-800">
                  {n}
                </span>
              </div>
            );
          })}
        </div>

        {devEntries.length > 0 && (
          <div className="min-w-0">
            <p className="mb-0.5 text-[10px] text-slate-500">Dev cards</p>
            <div className="flex flex-wrap gap-1">
              {devEntries.map(([k, n]) => (
                <div
                  key={k}
                  className="rounded border border-violet-200 bg-violet-50 px-1.5 py-0.5 text-[10px] text-violet-900"
                >
                  <span className="font-medium">{k}</span> ×{n}
                </div>
              ))}
            </div>
          </div>
        )}

        <div className="min-w-0">
          <p className="mb-0.5 text-[10px] text-slate-500">Pieces in supply</p>
          <div className="flex flex-wrap gap-2 text-[11px] text-slate-700">
            <span>
              Roads {me.pieceReserve.road ?? buildables["road"] ?? 0}
            </span>
            <span>
              Settlements{" "}
              {me.pieceReserve.settlement ?? buildables["settlement"] ?? 0}
            </span>
            <span>
              Cities {me.pieceReserve.city ?? buildables["city"] ?? 0}
            </span>
          </div>
        </div>
      </div>
    </div>
  );
}
