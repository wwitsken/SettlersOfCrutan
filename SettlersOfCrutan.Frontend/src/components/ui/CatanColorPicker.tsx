const COLORS = ["red", "blue", "white", "orange", "green"] as const;

const COLOR_HEX: Record<string, string> = {
  red: "#c94a3a",
  blue: "#3b6ea8",
  white: "#f3ecd6",
  orange: "#e49246",
  green: "#6b8e4e",
};

interface CatanColorPickerProps {
  value: string;
  onChange: (color: string) => void;
  taken?: string[];
}

export default function CatanColorPicker({
  value,
  onChange,
  taken = [],
}: CatanColorPickerProps) {
  return (
    <div
      className="inline-flex gap-1.5 rounded-full border-2 border-dashed border-(--ink-soft) bg-white/30 px-2 py-1"
      role="radiogroup"
      aria-label="Pick your color"
    >
      {COLORS.map((c) => {
        const isTaken = taken.includes(c) && c !== value;
        const isSelected = value === c;
        return (
          <button
            key={c}
            type="button"
            aria-label={c}
            disabled={isTaken}
            onClick={() => onChange(c)}
            className="cursor-pointer h-5 w-5 rounded-full border-2 border-(--ink) transition-transform hover:scale-110 disabled:cursor-not-allowed disabled:opacity-30"
            style={{
              background: COLOR_HEX[c],
              boxShadow: isSelected
                ? "0 0 0 2px var(--parchment), 0 0 0 4px var(--ink)"
                : undefined,
            }}
          />
        );
      })}
    </div>
  );
}
