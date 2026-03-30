import { useEffect, useState } from "react";
import type { PrivateGameInfo } from "../../domain/game/privateGame";
import type { TradeOffer } from "../../domain/game/tradeOffer";
import { postAcceptTrade } from "../../api/gameCommands";

const btnSm =
  "rounded-md border px-2.5 py-1 text-xs font-medium transition disabled:cursor-not-allowed disabled:opacity-40";

function resourceLines(resources: Record<string, number>): string[] {
  return Object.entries(resources)
    .filter(([, n]) => n > 0)
    .map(([t, n]) => `${n}× ${t}`);
}

function canPayRequest(
  hand: Record<string, number>,
  requested: Record<string, number>,
): boolean {
  for (const [t, need] of Object.entries(requested)) {
    if (need <= 0) continue;
    if ((hand[t] ?? 0) < need) return false;
  }
  return Object.values(requested).some((n) => n > 0);
}

type Props = {
  show: boolean;
  gameId: string;
  offer: TradeOffer;
  proposerDisplayName: string;
  privateGame: PrivateGameInfo;
};

/**
 * Strip above the board for players who are not the trade proposer.
 */
export function IncomingTradeBar({
  show,
  gameId,
  offer,
  proposerDisplayName,
  privateGame,
}: Props) {
  const [panelOpen, setPanelOpen] = useState(false);
  const [busy, setBusy] = useState(false);
  const [err, setErr] = useState<string | null>(null);

  useEffect(() => {
    setPanelOpen(false);
    setErr(null);
  }, [offer.id]);

  if (!show) return null;

  const hand = privateGame.myHand.resources;
  const canAccept = canPayRequest(hand, offer.requestedResources);
  const give = resourceLines(offer.requestedResources);
  const receive = resourceLines(offer.offeredResources);

  const accept = async () => {
    setBusy(true);
    setErr(null);
    const r = await postAcceptTrade(gameId, offer.id);
    setBusy(false);
    if (r.ok) setPanelOpen(false);
    else setErr(r.errorMessage);
  };

  return (
    <div className="relative z-20 rounded-lg border border-amber-200 bg-amber-50 px-3 py-2 shadow-sm">
      <button
        type="button"
        className="flex w-full items-center justify-between gap-2 text-left text-sm text-amber-950"
        onClick={() => setPanelOpen((o) => !o)}
        aria-expanded={panelOpen}
      >
        <span>
          <span className="font-medium">{proposerDisplayName}</span> proposed
          a trade ·{" "}
          <span className="text-amber-800 underline decoration-amber-600/60">
            {panelOpen ? "Hide" : "Review"}
          </span>
        </span>
      </button>

      {panelOpen && (
        <div
          className="absolute left-0 right-0 top-full z-30 mt-1 rounded-lg border border-slate-200 bg-white p-3 shadow-lg"
          role="dialog"
          aria-label="Trade offer details"
        >
          <p className="text-xs font-medium uppercase tracking-wide text-slate-500">
            You give
          </p>
          <ul className="mt-1 list-inside list-disc text-sm text-slate-900">
            {give.length ? (
              give.map((line) => <li key={line}>{line}</li>)
            ) : (
              <li className="list-none text-slate-500">—</li>
            )}
          </ul>
          <p className="mt-3 text-xs font-medium uppercase tracking-wide text-slate-500">
            You receive
          </p>
          <ul className="mt-1 list-inside list-disc text-sm text-slate-900">
            {receive.length ? (
              receive.map((line) => <li key={line}>{line}</li>)
            ) : (
              <li className="list-none text-slate-500">—</li>
            )}
          </ul>

          {err && (
            <p className="mt-2 text-sm text-red-700" role="alert">
              {err}
            </p>
          )}

          <div className="mt-3 flex flex-wrap gap-2">
            <button
              type="button"
              className={`${btnSm} border-slate-300 bg-white text-slate-800`}
              disabled={busy}
              onClick={() => {
                setPanelOpen(false);
                setErr(null);
              }}
            >
              Close
            </button>
            <button
              type="button"
              className={`${btnSm} border-emerald-600 bg-emerald-600 text-white hover:bg-emerald-700`}
              disabled={busy || !canAccept}
              title={
                !canAccept ? "Insufficient resources to accept this trade" : ""
              }
              onClick={() => void accept()}
            >
              {busy ? "Accepting…" : "Accept trade"}
            </button>
          </div>
        </div>
      )}
    </div>
  );
}
