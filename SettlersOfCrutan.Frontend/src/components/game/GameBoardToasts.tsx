import { useEffect, useRef } from "react";
import { useGameStore, type GameToast } from "../../stores/game";

const AUTO_DISMISS_MS = 5200;

function ToastLine({
  toast,
  onDismiss,
}: {
  toast: GameToast;
  onDismiss: (id: string) => void;
}) {
  const onDismissRef = useRef(onDismiss);
  onDismissRef.current = onDismiss;

  useEffect(() => {
    const id = toast.id;
    const t = window.setTimeout(
      () => onDismissRef.current(id),
      AUTO_DISMISS_MS,
    );
    return () => window.clearTimeout(t);
  }, [toast.id]);

  return (
    <button
      type="button"
      className="game-toast-animate pointer-events-auto max-w-2xs cursor-pointer rounded-xl border border-amber-700/50 bg-stone-900/95 px-4 py-1 text-center text-sm font-medium text-stone-100 shadow-xl backdrop-blur-sm transition hover:bg-stone-800/95 focus:outline-none focus-visible:ring-2 focus-visible:ring-amber-500/80"
      onClick={() => onDismiss(toast.id)}
    >
      {toast.message}
    </button>
  );
}

/**
 * Centered stack over the board region; click a toast to dismiss early.
 */
export function GameBoardToasts() {
  const toasts = useGameStore((s) => s.toasts);
  const dismiss = useGameStore((s) => s.dismissToast);

  return (
    <div
      className="pointer-events-none absolute inset-x-0 top-0 z-20 flex flex-col items-left gap-2 pl-3 pt-3"
      aria-live="polite"
      aria-relevant="additions"
    >
      {toasts.map((t) => (
        <ToastLine key={t.id} toast={t} onDismiss={dismiss} />
      ))}
    </div>
  );
}
