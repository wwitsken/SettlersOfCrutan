import { useState } from "react";
import type { ResourceCardType } from "../../domain/game/gameTypes";

const OPTIONS: ResourceCardType[] = [
  "brick",
  "lumber",
  "wool",
  "grain",
  "ore",
];

type MonopolyProps = {
  mode: "monopoly";
  open: boolean;
  busy: boolean;
  error: string | null;
  onConfirm: (resourceType: ResourceCardType) => void;
  onCancel: () => void;
};

type YearProps = {
  mode: "yearOfPlenty";
  open: boolean;
  busy: boolean;
  error: string | null;
  onConfirm: (a: ResourceCardType, b: ResourceCardType) => void;
  onCancel: () => void;
};

type Props = MonopolyProps | YearProps;

export function DevCardResourceDialog(props: Props) {
  const [a, setA] = useState<ResourceCardType>("brick");
  const [b, setB] = useState<ResourceCardType>("brick");

  if (!props.open) return null;

  const title =
    props.mode === "monopoly" ? "Monopoly" : "Year of plenty";
  const subtitle =
    props.mode === "monopoly"
      ? "Pick the resource to take from all opponents."
      : "Pick two resources from the bank.";

  return (
    <div
      className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 p-4"
      role="dialog"
      aria-modal="true"
    >
      <div className="w-full max-w-sm rounded-xl border border-slate-200 bg-white p-4 shadow-lg">
        <h2 className="text-lg font-semibold text-slate-900">{title}</h2>
        <p className="mt-1 text-sm text-slate-600">{subtitle}</p>

        {props.mode === "monopoly" ? (
          <select
            className="mt-3 w-full rounded border border-slate-300 px-2 py-2 text-sm"
            value={a}
            onChange={(e) => setA(e.target.value as ResourceCardType)}
          >
            {OPTIONS.map((o) => (
              <option key={o} value={o}>
                {o}
              </option>
            ))}
          </select>
        ) : (
          <div className="mt-3 flex flex-col gap-2">
            <label className="text-xs text-slate-600">
              Resource 1
              <select
                className="mt-0.5 w-full rounded border border-slate-300 px-2 py-2 text-sm"
                value={a}
                onChange={(e) => setA(e.target.value as ResourceCardType)}
              >
                {OPTIONS.map((o) => (
                  <option key={o} value={o}>
                    {o}
                  </option>
                ))}
              </select>
            </label>
            <label className="text-xs text-slate-600">
              Resource 2
              <select
                className="mt-0.5 w-full rounded border border-slate-300 px-2 py-2 text-sm"
                value={b}
                onChange={(e) => setB(e.target.value as ResourceCardType)}
              >
                {OPTIONS.map((o) => (
                  <option key={o} value={o}>
                    {o}
                  </option>
                ))}
              </select>
            </label>
          </div>
        )}

        {props.error && (
          <p className="mt-2 text-sm text-red-700">{props.error}</p>
        )}

        <div className="mt-4 flex justify-end gap-2">
          <button
            type="button"
            className="rounded-md border border-slate-300 px-3 py-1.5 text-sm"
            disabled={props.busy}
            onClick={props.onCancel}
          >
            Cancel
          </button>
          <button
            type="button"
            className="rounded-md bg-slate-900 px-3 py-1.5 text-sm font-medium text-white disabled:opacity-40"
            disabled={props.busy}
            onClick={() =>
              props.mode === "monopoly"
                ? props.onConfirm(a)
                : props.onConfirm(a, b)
            }
          >
            {props.busy ? "…" : "Confirm"}
          </button>
        </div>
      </div>
    </div>
  );
}
