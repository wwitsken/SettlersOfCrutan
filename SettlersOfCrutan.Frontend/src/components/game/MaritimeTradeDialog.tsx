import { useEffect, useMemo, useState } from "react";
import type { Game } from "../../domain/game/game";
import type { PrivateGameInfo } from "../../domain/game/privateGame";
import type { ResourceCardType } from "../../domain/game/gameTypes";
import {
  postMaritimeTrade2to1,
  postMaritimeTrade3to1,
  postMaritimeTrade4to1,
} from "../../api/gameCommands";

const TRADABLE: ResourceCardType[] = [
  "brick",
  "lumber",
  "wool",
  "grain",
  "ore",
];

export type MaritimeRatio = 4 | 3 | 2;

type Props = {
  open: boolean;
  ratio: MaritimeRatio;
  gameId: string;
  game: Game;
  privateGame: PrivateGameInfo;
  onClose: () => void;
};

export function MaritimeTradeDialog({
  open,
  ratio,
  gameId,
  game,
  privateGame,
  onClose,
}: Props) {
  const [discard, setDiscard] = useState<ResourceCardType | "">("");
  const [request, setRequest] = useState<ResourceCardType | "">("");
  const [busy, setBusy] = useState(false);
  const [err, setErr] = useState<string | null>(null);

  const hand = privateGame.myHand.resources;
  const bank = game.bankResourceHand;

  useEffect(() => {
    if (open) {
      setDiscard("");
      setRequest("");
      setErr(null);
    }
  }, [open, ratio]);

  const eligibleDiscard = useMemo(
    () =>
      TRADABLE.filter((t) => (hand[t] ?? 0) >= ratio).sort((a, b) =>
        a.localeCompare(b),
      ),
    [hand, ratio],
  );

  const eligibleRequest = useMemo(
    () =>
      TRADABLE.filter((t) => (bank[t] ?? 0) > 0).sort((a, b) =>
        a.localeCompare(b),
      ),
    [bank],
  );

  if (!open) return null;

  const label =
    ratio === 4 ? "4:1" : ratio === 3 ? "3:1" : "2:1";

  const submit = async () => {
    if (!discard || !request) {
      setErr("Choose one resource to give and one to receive.");
      return;
    }
    setBusy(true);
    setErr(null);
    const d = discard as ResourceCardType;
    const req = request as ResourceCardType;
    const r =
      ratio === 4
        ? await postMaritimeTrade4to1(gameId, d, req)
        : ratio === 3
          ? await postMaritimeTrade3to1(gameId, d, req)
          : await postMaritimeTrade2to1(gameId, d, req);
    setBusy(false);
    if (r.ok) onClose();
    else setErr(r.errorMessage);
  };

  return (
    <div
      className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 p-4"
      role="dialog"
      aria-modal="true"
      aria-labelledby="maritime-title"
    >
      <div className="w-full max-w-md rounded-xl border border-slate-200 bg-white p-4 shadow-lg">
        <h2 id="maritime-title" className="text-lg font-semibold text-slate-900">
          Maritime trade ({label})
        </h2>
        <p className="mt-1 text-sm text-slate-600">
          Give {ratio} cards of a single type from your hand for 1 card from the
          bank. Ports and bank stock are enforced by the server.
        </p>

        <div className="mt-4 space-y-3">
          <div>
            <label
              htmlFor="maritime-discard"
              className="text-xs font-medium text-slate-600"
            >
              Give ({ratio}× one type)
            </label>
            <select
              id="maritime-discard"
              className="mt-1 w-full rounded-md border border-slate-300 bg-white px-2 py-2 text-sm"
              disabled={busy}
              value={discard}
              onChange={(e) =>
                setDiscard((e.target.value || "") as ResourceCardType | "")
              }
            >
              <option value="">Select resource…</option>
              {eligibleDiscard.map((t) => (
                <option key={t} value={t}>
                  {t} (have {hand[t] ?? 0})
                </option>
              ))}
            </select>
          </div>
          <div>
            <label
              htmlFor="maritime-request"
              className="text-xs font-medium text-slate-600"
            >
              Receive (1 card)
            </label>
            <select
              id="maritime-request"
              className="mt-1 w-full rounded-md border border-slate-300 bg-white px-2 py-2 text-sm"
              disabled={busy}
              value={request}
              onChange={(e) =>
                setRequest((e.target.value || "") as ResourceCardType | "")
              }
            >
              <option value="">Select resource…</option>
              {eligibleRequest.map((t) => (
                <option key={t} value={t}>
                  {t} (bank {bank[t] ?? 0})
                </option>
              ))}
            </select>
          </div>
        </div>

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
            onClick={onClose}
          >
            Cancel
          </button>
          <button
            type="button"
            className="rounded-md bg-slate-900 px-3 py-1.5 text-sm font-medium text-white disabled:opacity-40"
            disabled={busy || !discard || !request}
            onClick={() => void submit()}
          >
            {busy ? "Trading…" : "Trade"}
          </button>
        </div>
      </div>
    </div>
  );
}
