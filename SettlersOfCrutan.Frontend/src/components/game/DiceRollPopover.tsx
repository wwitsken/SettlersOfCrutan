import { useEffect, useRef, useState } from "react";
import type { CurrentDiceRoll } from "../../domain/game/gameTypes";
import { GAME_TOAST_AUTO_DISMISS_MS } from "./gameToastTiming";

const PIP_POSITIONS: Record<number, [string, string][]> = {
  1: [["50%", "50%"]],
  2: [["75%", "25%"], ["25%", "75%"]],
  3: [["75%", "25%"], ["50%", "50%"], ["25%", "75%"]],
  4: [["25%", "25%"], ["75%", "25%"], ["25%", "75%"], ["75%", "75%"]],
  5: [["25%", "25%"], ["75%", "25%"], ["50%", "50%"], ["25%", "75%"], ["75%", "75%"]],
  6: [["25%", "20%"], ["25%", "50%"], ["25%", "80%"], ["75%", "20%"], ["75%", "50%"], ["75%", "80%"]],
};

function rollKey(roll: CurrentDiceRoll): string {
  return `${roll.die1},${roll.die2}`;
}

function DieFace({ value }: { value: number }) {
  const pips = PIP_POSITIONS[value] ?? [];
  return (
    <div className="relative h-10 w-10 rounded-lg border-2 border-amber-700/60 bg-stone-100 shadow-md">
      {pips.map(([left, top], i) => (
        <span
          key={i}
          className="absolute h-2 w-2 -translate-x-1/2 -translate-y-1/2 rounded-full bg-stone-800"
          style={{ left, top }}
        />
      ))}
    </div>
  );
}

export function DiceRollPopover({ roll }: { roll: CurrentDiceRoll | undefined }) {
  const [visible, setVisible] = useState(false);
  const [shownRoll, setShownRoll] = useState<CurrentDiceRoll | null>(null);
  const lastShownKeyRef = useRef<string | null>(null);

  useEffect(() => {
    if (!roll) {
      setVisible(false);
      setShownRoll(null);
      lastShownKeyRef.current = null;
      return;
    }

    const key = rollKey(roll);
    if (lastShownKeyRef.current === key) return;

    lastShownKeyRef.current = key;
    setShownRoll(roll);
    setVisible(true);

    const t = window.setTimeout(
      () => setVisible(false),
      GAME_TOAST_AUTO_DISMISS_MS,
    );
    return () => window.clearTimeout(t);
  }, [roll]);

  if (!visible || !shownRoll) return null;

  const total = shownRoll.die1 + shownRoll.die2;

  return (
    <div className="pointer-events-none absolute inset-x-0 top-3 z-20 flex justify-center">
      <button
        type="button"
        className="game-toast-animate pointer-events-auto flex cursor-pointer items-center gap-2 rounded-xl border border-amber-700/50 bg-stone-900/90 px-4 py-2 shadow-xl backdrop-blur-sm transition hover:bg-stone-800/95 focus:outline-none focus-visible:ring-2 focus-visible:ring-amber-500/80"
        onClick={() => setVisible(false)}
        aria-label={`Dice roll ${shownRoll.die1} and ${shownRoll.die2}, total ${total}. Click to dismiss.`}
      >
        <DieFace value={shownRoll.die1} />
        <DieFace value={shownRoll.die2} />
        <span className="ml-1 text-lg font-bold text-amber-300">{total}</span>
      </button>
    </div>
  );
}
