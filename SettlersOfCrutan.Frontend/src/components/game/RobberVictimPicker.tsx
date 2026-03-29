type Victim = { id: string; displayName: string };

type Props = {
  open: boolean;
  victims: Victim[];
  busy: boolean;
  error: string | null;
  onPick: (victimId: string) => void;
  onCancel: () => void;
};

export function RobberVictimPicker({
  open,
  victims,
  busy,
  error,
  onPick,
  onCancel,
}: Props) {
  if (!open) return null;

  return (
    <div
      className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 p-4"
      role="dialog"
      aria-modal="true"
      aria-labelledby="robber-victim-title"
    >
      <div className="w-full max-w-sm rounded-xl border border-slate-200 bg-white p-4 shadow-lg">
        <h2
          id="robber-victim-title"
          className="text-lg font-semibold text-slate-900"
        >
          Steal from
        </h2>
        <p className="mt-1 text-sm text-slate-600">
          Choose a player with a building on the robber hex.
        </p>

        {victims.length === 0 ? (
          <p className="mt-3 text-sm text-slate-500">
            No eligible opponents on this hex.
          </p>
        ) : (
          <ul className="mt-3 space-y-1">
            {victims.map((v) => (
              <li key={v.id}>
                <button
                  type="button"
                  disabled={busy}
                  className="w-full rounded-lg border border-slate-200 px-3 py-2 text-left text-sm hover:bg-slate-50 disabled:opacity-40"
                  onClick={() => onPick(v.id)}
                >
                  {v.displayName || v.id}
                </button>
              </li>
            ))}
          </ul>
        )}

        {error && (
          <p className="mt-2 text-sm text-red-700" role="alert">
            {error}
          </p>
        )}

        <div className="mt-4 flex justify-end">
          <button
            type="button"
            className="rounded-md border border-slate-300 px-3 py-1.5 text-sm text-slate-700"
            disabled={busy}
            onClick={onCancel}
          >
            Cancel
          </button>
        </div>
      </div>
    </div>
  );
}
