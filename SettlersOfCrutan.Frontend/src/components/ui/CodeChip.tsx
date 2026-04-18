import { useState } from "react";
import CatanButton from "./CatanButton";

interface CodeChipProps {
  code: string;
}

export default function CodeChip({ code }: CodeChipProps) {
  const [hint, setHint] = useState<string | null>(null);

  const handleCopy = async () => {
    try {
      await navigator.clipboard.writeText(code);
      setHint("✓ copied!");
    } catch {
      setHint("Could not copy");
    }
    setTimeout(() => setHint(null), 1600);
  };

  return (
    <div className="flex items-center gap-2 flex-wrap">
      <span
        className="inline-flex items-center rounded-xl border-2 border-(--ink) bg-(--parchment-2) px-3 py-2 shadow-[2px_2px_0_var(--ink)] tracking-widest"
        style={{ fontFamily: "var(--font-mono)", fontSize: "1.1rem" }}
      >
        {code}
      </span>
      <CatanButton size="md" onClick={() => void handleCopy()}>
        {hint ? (
          <span style={{ color: "var(--catan-accent)", fontWeight: 700 }}>
            {hint}
          </span>
        ) : (
          "copy"
        )}
      </CatanButton>
    </div>
  );
}
