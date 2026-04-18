import { useState, useEffect } from "react";

interface CatanNameEditProps {
  value: string;
  onChange: (name: string) => void;
  placeholder?: string;
}

export default function CatanNameEdit({ value, onChange, placeholder = "Your name" }: CatanNameEditProps) {
  const [local, setLocal] = useState(value);
  useEffect(() => setLocal(value), [value]);

  return (
    <input
      className="min-w-36 bg-transparent outline-none"
      style={{
        fontFamily: "var(--font-serif)",
        fontSize: "1.1rem",
        color: "var(--ink)",
        borderBottom: "2px dashed var(--ink-soft)",
        padding: "2px 4px",
      }}
      value={local}
      placeholder={placeholder}
      onChange={(e) => setLocal(e.target.value)}
      onBlur={() => onChange(local)}
      onKeyDown={(e) => {
        if (e.key === "Enter") e.currentTarget.blur();
      }}
    />
  );
}
