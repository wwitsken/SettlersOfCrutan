import { useEffect, useLayoutEffect, useMemo, useRef, useState } from "react";
import type { Game } from "../../domain/game/game";
import type { PrivateGameInfo } from "../../domain/game/privateGame";
import type { ResourceCardType } from "../../domain/game/gameTypes";
import type { MaritimeRatio } from "../../domain/game/maritimeEligibility";
import { RESOURCE_META } from "../../constants/catanMeta";
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

const VIEW_MARGIN = 8;
const GAP_BELOW_ANCHOR = 10;

function collectScrollContainers(start: HTMLElement): EventTarget[] {
  const targets: EventTarget[] = [window];
  let el: HTMLElement | null = start;
  while (el) {
    const st = getComputedStyle(el);
    if (
      /(auto|scroll|overlay)/.test(st.overflowY) ||
      /(auto|scroll|overlay)/.test(st.overflowX)
    ) {
      targets.push(el);
    }
    el = el.parentElement;
  }
  return targets;
}

type Props = {
  gameId: string;
  game: Game;
  privateGame: PrivateGameInfo;
  discard: ResourceCardType;
  /** When null, no maritime is possible for this discard right now (hand + ports). */
  ratio: MaritimeRatio | null;
  anchorEl: HTMLButtonElement;
  onClose: () => void;
};

export function ResourceMaritimePopover({
  gameId,
  game,
  privateGame,
  discard,
  ratio,
  anchorEl,
  onClose,
}: Props) {
  const panelRef = useRef<HTMLDivElement>(null);
  const [request, setRequest] = useState<ResourceCardType | "">("");
  const [busy, setBusy] = useState(false);
  const [err, setErr] = useState<string | null>(null);
  const [panelPos, setPanelPos] = useState({ top: VIEW_MARGIN, left: VIEW_MARGIN });

  const hand = privateGame.myHand.resources;
  const bank = game.bankResourceHand;

  const eligibleRequest = useMemo(
    () =>
      TRADABLE.filter((t) => (bank[t] ?? 0) > 0).sort((a, b) =>
        a.localeCompare(b),
      ),
    [bank],
  );

  const ratioLabel =
    ratio === null ? null : ratio === 4 ? "4:1" : ratio === 3 ? "3:1" : "2:1";
  const discardLabel = RESOURCE_META[discard]?.label ?? discard;
  const have = hand[discard] ?? 0;

  useLayoutEffect(() => {
    const panel = panelRef.current;
    if (!panel) return;
    if (!anchorEl.isConnected) {
      queueMicrotask(onClose);
      return;
    }

    const anchor = anchorEl.getBoundingClientRect();
    const ph = panel.offsetHeight;
    const pw = panel.offsetWidth;
    const m = VIEW_MARGIN;

    let top = anchor.bottom + GAP_BELOW_ANCHOR;
    const spaceBelow = window.innerHeight - top - m;
    const spaceAbove = anchor.top - m;
    if (ph > spaceBelow && spaceAbove >= ph + GAP_BELOW_ANCHOR + m) {
      top = anchor.top - ph - GAP_BELOW_ANCHOR;
    } else if (top + ph > window.innerHeight - m) {
      top = Math.max(m, window.innerHeight - ph - m);
    }

    let left = anchor.left + anchor.width / 2 - pw / 2;
    left = Math.max(m, Math.min(left, window.innerWidth - pw - m));

    setPanelPos({ top, left });
  }, [anchorEl, onClose, ratio, discard, request, err, eligibleRequest.length]);

  useEffect(() => {
    const onKey = (e: KeyboardEvent) => {
      if (e.key === "Escape") onClose();
    };
    const onPointer = (e: MouseEvent | PointerEvent) => {
      const el = panelRef.current;
      if (!el) return;
      const t = e.target as Node | null;
      if (t && !el.contains(t)) onClose();
    };
    window.addEventListener("keydown", onKey);
    window.addEventListener("pointerdown", onPointer, true);
    return () => {
      window.removeEventListener("keydown", onKey);
      window.removeEventListener("pointerdown", onPointer, true);
    };
  }, [onClose]);

  useEffect(() => {
    const opts: AddEventListenerOptions = { capture: true, passive: true };
    const close = () => onClose();
    const targets = collectScrollContainers(anchorEl);
    for (const t of targets) {
      t.addEventListener("scroll", close, opts);
    }
    const vv = window.visualViewport;
    vv?.addEventListener("scroll", close);
    return () => {
      for (const t of targets) {
        t.removeEventListener("scroll", close, opts);
      }
      vv?.removeEventListener("scroll", close);
    };
  }, [anchorEl, onClose]);

  const submit = async () => {
    if (ratio === null) return;
    if (!request) {
      setErr("Choose a resource to receive from the bank.");
      return;
    }
    setBusy(true);
    setErr(null);
    const r =
      ratio === 4
        ? await postMaritimeTrade4to1(gameId, discard, request as ResourceCardType)
        : ratio === 3
          ? await postMaritimeTrade3to1(gameId, discard, request as ResourceCardType)
          : await postMaritimeTrade2to1(gameId, discard, request as ResourceCardType);
    setBusy(false);
    if (r.ok) onClose();
    else setErr(r.errorMessage);
  };

  return (
    <div className="fixed inset-0 z-[60] pointer-events-none">
      <div
        ref={panelRef}
        className="pointer-events-auto absolute w-[min(100vw-16px,280px)] rounded-xl border border-stone-600 bg-stone-900 p-3 shadow-xl"
        style={{ top: panelPos.top, left: panelPos.left }}
        role="dialog"
        aria-modal="true"
        aria-labelledby="maritime-pop-title"
      >
        <h2
          id="maritime-pop-title"
          className="text-sm font-semibold text-stone-100"
        >
          {ratioLabel ? `Maritime trade (${ratioLabel})` : "Maritime trade"}
        </h2>
        {ratio === null ? (
          <p className="mt-2 text-xs text-stone-400">
            You cannot maritime with <span className="text-stone-200">{discardLabel}</span>{" "}
            right now (need enough cards and/or a port for a better rate). You have{" "}
            <span className="text-stone-200">{have}</span>.
          </p>
        ) : (
          <>
            <p className="mt-1 text-xs text-stone-400">
              Give <span className="text-stone-200">{ratio}× {discardLabel}</span>{" "}
              (you have {have}) for 1 from the bank.
            </p>

            <div className="mt-3">
              <label
                htmlFor="maritime-pop-receive"
                className="text-xs font-medium text-stone-500"
              >
                Receive
              </label>
              <select
                id="maritime-pop-receive"
                className="mt-1 w-full rounded-md border border-stone-600 bg-stone-950 px-2 py-2 text-sm text-stone-100"
                disabled={busy}
                value={request}
                onChange={(e) =>
                  setRequest((e.target.value || "") as ResourceCardType | "")
                }
              >
                <option value="">Select resource…</option>
                {eligibleRequest.map((t) => (
                  <option key={t} value={t}>
                    {RESOURCE_META[t]?.label ?? t} (bank {bank[t] ?? 0})
                  </option>
                ))}
              </select>
            </div>

            {err && (
              <p className="mt-2 text-xs text-red-400" role="alert">
                {err}
              </p>
            )}
          </>
        )}

        <div className="mt-3 flex justify-end gap-2">
          <button
            type="button"
            className="rounded-md border border-stone-600 px-2 py-1 text-xs text-stone-300 hover:bg-stone-800"
            disabled={busy}
            onClick={onClose}
          >
            {ratio === null ? "Close" : "Cancel"}
          </button>
          {ratio !== null && (
            <button
              type="button"
              className="rounded-md bg-stone-200 px-2 py-1 text-xs font-medium text-stone-900 disabled:opacity-40"
              disabled={busy || !request}
              onClick={() => void submit()}
            >
              {busy ? "Trading…" : "Trade"}
            </button>
          )}
        </div>
      </div>
    </div>
  );
}
