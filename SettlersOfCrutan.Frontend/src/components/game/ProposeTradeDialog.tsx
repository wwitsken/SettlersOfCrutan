import { useMemo, useState } from "react";
import type { PrivateGameInfo } from "../../domain/game/privateGame";
import type { ResourceCardType } from "../../domain/game/gameTypes";
import { postOfferTrade } from "../../api/gameCommands";
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
  privateGame: PrivateGameInfo;
  onClose: () => void;
};

export function ProposeTradeDialog({
  open,
  gameId,
  privateGame,
  onClose,
}: Props) {
  const [offerCounts, setOfferCounts] = useState<Record<string, number>>({});
  const [wantCounts, setWantCounts] = useState<Record<string, number>>({});
  const [busy, setBusy] = useState(false);
  const [err, setErr] = useState<string | null>(null);

  const hand = privateGame.myHand.resources;

  const offerTotal = useMemo(
    () => Object.values(offerCounts).reduce((a, b) => a + b, 0),
    [offerCounts],
  );
  const wantTotal = useMemo(
    () => Object.values(wantCounts).reduce((a, b) => a + b, 0),
    [wantCounts],
  );

  if (!open) return null;

  const setOffer = (t: ResourceCardType, next: number) => {
    const max = hand[t] ?? 0;
    const clamped = Math.max(0, Math.min(max, next));
    setOfferCounts((c) => {
      const n = { ...c };
      if (clamped === 0) delete n[t];
      else n[t] = clamped;
      return n;
    });
  };

  const setWant = (t: ResourceCardType, next: number) => {
    const clamped = Math.max(0, next);
    setWantCounts((c) => {
      const n = { ...c };
      if (clamped === 0) delete n[t];
      else n[t] = clamped;
      return n;
    });
  };

  const toDtoList = (
    counts: Record<string, number>,
  ): components["schemas"]["ResourceCardAmountDto"][] =>
    Object.entries(counts)
      .filter(([, q]) => q > 0)
      .map(([type, quantity]) => ({
        type: type as ResourceCardType,
        quantity,
      }));

  const submit = async () => {
    const offered = toDtoList(offerCounts);
    const requested = toDtoList(wantCounts);
    if (offered.length === 0 || requested.length === 0) {
      setErr("Offer at least one resource on each side.");
      return;
    }
    setBusy(true);
    setErr(null);
    const r = await postOfferTrade(gameId, requested, offered);
    setBusy(false);
    if (r.ok) {
      setOfferCounts({});
      setWantCounts({});
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
      aria-labelledby="propose-trade-title"
    >
      <div className="max-h-[90vh] w-full max-w-lg overflow-auto rounded-xl border border-slate-200 bg-white p-4 shadow-lg">
        <h2
          id="propose-trade-title"
          className="text-lg font-semibold text-slate-900"
        >
          Propose trade
        </h2>
        <p className="mt-1 text-sm text-slate-600">
          Choose what you offer (from your hand) and what you Request from
          others. One row per resource type.
        </p>

        <div className="mt-4 grid gap-6 sm:grid-cols-2">
          <div>
            <h3 className="text-sm font-medium text-slate-800">You offer</h3>
            <ul className="mt-2 space-y-2">
              {TRADABLE.map((t) => {
                const have = hand[t] ?? 0;
                const pick = offerCounts[t] ?? 0;
                return (
                  <li
                    key={t}
                    className="flex items-center justify-between gap-2 text-sm"
                  >
                    <span className="capitalize text-slate-800">
                      {t}{" "}
                      <span className="text-slate-500">(have {have})</span>
                    </span>
                    <div className="flex items-center gap-1">
                      <button
                        type="button"
                        className="h-8 w-8 rounded border border-slate-300 text-slate-700 disabled:opacity-30"
                        disabled={pick <= 0 || busy}
                        onClick={() => setOffer(t, pick - 1)}
                      >
                        −
                      </button>
                      <span className="w-6 text-center tabular-nums">{pick}</span>
                      <button
                        type="button"
                        className="h-8 w-8 rounded border border-slate-300 text-slate-700 disabled:opacity-30"
                        disabled={pick >= have || busy}
                        onClick={() => setOffer(t, pick + 1)}
                      >
                        +
                      </button>
                    </div>
                  </li>
                );
              })}
            </ul>
          </div>
          <div>
            <h3 className="text-sm font-medium text-slate-800">
              You request
            </h3>
            <ul className="mt-2 space-y-2">
              {TRADABLE.map((t) => {
                const pick = wantCounts[t] ?? 0;
                return (
                  <li
                    key={t}
                    className="flex items-center justify-between gap-2 text-sm"
                  >
                    <span className="capitalize text-slate-800">{t}</span>
                    <div className="flex items-center gap-1">
                      <button
                        type="button"
                        className="h-8 w-8 rounded border border-slate-300 text-slate-700 disabled:opacity-30"
                        disabled={pick <= 0 || busy}
                        onClick={() => setWant(t, pick - 1)}
                      >
                        −
                      </button>
                      <span className="w-6 text-center tabular-nums">{pick}</span>
                      <button
                        type="button"
                        className="h-8 w-8 rounded border border-slate-300 text-slate-700 disabled:opacity-30"
                        disabled={busy}
                        onClick={() => setWant(t, pick + 1)}
                      >
                        +
                      </button>
                    </div>
                  </li>
                );
              })}
            </ul>
          </div>
        </div>

        <p className="mt-3 text-xs text-slate-500">
          Offer total: {offerTotal} cards · Request: {wantTotal} cards (both must
          be &gt; 0).
        </p>

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
              setOfferCounts({});
              setWantCounts({});
              setErr(null);
              onClose();
            }}
          >
            Cancel
          </button>
          <button
            type="button"
            className="rounded-md bg-slate-900 px-3 py-1.5 text-sm font-medium text-white disabled:opacity-40"
            disabled={busy || offerTotal === 0 || wantTotal === 0}
            onClick={() => void submit()}
          >
            {busy ? "Sending…" : "Send offer"}
          </button>
        </div>
      </div>
    </div>
  );
}
