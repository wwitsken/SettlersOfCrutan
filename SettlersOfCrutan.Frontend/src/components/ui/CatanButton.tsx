import type { ButtonHTMLAttributes, ReactNode } from "react";

type ButtonVariant = "default" | "primary" | "ghost";

interface CatanButtonProps extends ButtonHTMLAttributes<HTMLButtonElement> {
  variant?: ButtonVariant;
  size?: "sm" | "md";
  children: ReactNode;
}

const VARIANT_CLASSES: Record<ButtonVariant, string> = {
  default:
    "bg-(--parchment-2) text-(--ink) border-(--ink) hover:-translate-x-px hover:-translate-y-px",
  primary:
    "bg-(--catan-accent) text-[#fff7e3] border-(--ink) hover:-translate-x-px hover:-translate-y-px",
  ghost:
    "bg-transparent text-(--ink) border-(--ink-soft) hover:-translate-x-px hover:-translate-y-px",
};

const SIZE_CLASSES = {
  sm: "px-3 py-1 text-base",
  md: "px-4 py-2 text-lg",
};

export default function CatanButton({
  variant = "default",
  size = "md",
  className = "",
  children,
  disabled,
  ...rest
}: CatanButtonProps) {
  return (
    <button
      className={`cursor-pointer inline-flex items-center gap-2 rounded-xl border-2 shadow-[2px_2px_0_var(--ink)] transition-transform whitespace-nowrap
        font-(--font-hand) ${VARIANT_CLASSES[variant]} ${SIZE_CLASSES[size]}
        disabled:opacity-40 disabled:cursor-not-allowed disabled:hover:translate-x-0 disabled:hover:translate-y-0
        ${className}`}
      disabled={disabled}
      {...rest}
    >
      {children}
    </button>
  );
}
