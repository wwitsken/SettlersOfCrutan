import { useMemo, useState } from "react";
import type { PrivateGameInfo } from "../../domain/game/privateGame";
import type { ResourceCardType } from "../../domain/game/gameTypes";
import { postDiscardHalf } from "../../api/gameCommands";
import type { components } from "../../api/types";

const TRADABLE: ResourceCardType[] = [
  "brick",
  "lumber",
  "wool",
  "grain",
  "ore",
];

type Props = {
  open: boolean;
  gameId: string;
  required: number;
  privateGame: PrivateGameInfo;
  onClose: () => void;
};

export function DiscardHalfDialog({
  open,
  gameId,
  required,
  privateGame,
  onClose,
}: Props) {
  const [counts, setCounts] = useState<Record<string, number>>({});
  const [busy, setBusy] = useState(false);
  const [err, setErr] = useState<string | null>(null);

  const hand = privateGame.myHand.resources;

  const totalPicked = useMemo(
    () => Object.values(counts).reduce((a, b) => a + b, 0),
    [counts],
  );

  if (!open || required <= 0) return null;

  const setForType = (t: ResourceCardType, next: number) => {
    const max = hand[t] ?? 0;
    const clamped = Math.max(0, Math.min(max, next));
    setCounts((c) => {
      const n = { ...c };
      if (clamped === 0) delete n[t];
      else n[t] = clamped;
      return n;
    });
  };

  const submit = async () => {
    if (totalPicked !== required) return;
    setBusy(true);
    setErr(null);
    const discards: components["schemas"]["ResourceCardAmountDto"][] =
      Object.entries(counts).map(([type, quantity]) => ({
        type: type as ResourceCardType,
        quantity,
      }));
    const r = await postDiscardHalf(gameId, discards);
    setBusy(false);
    if (r.ok) {
      setCounts({});
      onClose();
    } else {
      setErr(r.errorMessage);
    }
  };

  return (
    <div
      className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 p-4"
      role="dialog"
      aria-modal="true"
      aria-labelledby="discard-title"
    >
      <div className="max-h-[90vh] w-full max-w-md overflow-auto rounded-xl border border-slate-200 bg-white p-4 shadow-lg">
        <h2 id="discard-title" className="text-lg font-semibold text-slate-900">
          Discard {required} card{required === 1 ? "" : "s"}
        </h2>
        <p className="mt-1 text-sm text-slate-600">
          Select exactly half your hand ({required} total). Picked: {totalPicked}{" "}
          / {required}.
        </p>

        <ul className="mt-4 space-y-3">
          {TRADABLE.map((t) => {
            const have = hand[t] ?? 0;
            const pick = counts[t] ?? 0;
            return (
              <li
                key={t}
                className="flex items-center justify-between gap-2 text-sm"
              >
                <span className="capitalize text-slate-800">
                  {t} <span className="text-slate-500">(have {have})</span>
                </span>
                <div className="flex items-center gap-1">
                  <button
                    type="button"
                    className="h-8 w-8 rounded border border-slate-300 text-slate-700 disabled:opacity-30"
                    disabled={pick <= 0 || busy}
                    onClick={() => setForType(t, pick - 1)}
                  >
                    −
                  </button>
                  <span className="w-6 text-center tabular-nums">{pick}</span>
                  <button
                    type="button"
                    className="h-8 w-8 rounded border border-slate-300 text-slate-700 disabled:opacity-30"
                    disabled={pick >= have || busy}
                    onClick={() => setForType(t, pick + 1)}
                  >
                    +
                  </button>
                </div>
              </li>
            );
          })}
        </ul>

        {err && (
          <p className="mt-3 text-sm text-red-700" role="alert">
            {err}
          </p>
        )}

        <div className="mt-4 flex justify-end gap-2">
          <button
            type="button"
            className="rounded-md border border-slate-300 px-3 py-1.5 text-sm text-slate-700"
            disabled={busy}
            onClick={() => {
              setCounts({});
              onClose();
            }}
          >
            Cancel
          </button>
          <button
            type="button"
            className="rounded-md bg-slate-900 px-3 py-1.5 text-sm font-medium text-white disabled:opacity-40"
            disabled={busy || totalPicked !== required}
            onClick={() => void submit()}
          >
            {busy ? "Discarding…" : "Discard"}
          </button>
        </div>
      </div>
    </div>
  );
}
