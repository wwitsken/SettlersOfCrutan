import type { DevelopmentCardType } from "./gameTypes";

const DEV_ORDER: DevelopmentCardType[] = [
  "knight",
  "monopoly",
  "roadBuilding",
  "yearOfPlenty",
  "victoryPoint",
];

const DEV_CARD_KEY_SET = new Set<string>(DEV_ORDER);

function normalizeDevCardTypeKey(raw: string): DevelopmentCardType | null {
  const trimmed = raw.trim();
  if (!trimmed) return null;
  const lower = trimmed.toLowerCase();
  if (DEV_CARD_KEY_SET.has(lower)) return lower as DevelopmentCardType;
  const camelFromPascal =
    trimmed.charAt(0).toLowerCase() + trimmed.slice(1);
  if (DEV_CARD_KEY_SET.has(camelFromPascal))
    return camelFromPascal as DevelopmentCardType;
  return null;
}

export function expandUnplayedDevCards(
  counts: Record<string, number>,
): { id: string; type: DevelopmentCardType }[] {
  const out: { id: string; type: DevelopmentCardType }[] = [];
  for (const t of DEV_ORDER) {
    const n = Math.max(0, Math.floor(Number(counts[t] ?? 0)));
    for (let i = 0; i < n; i++) out.push({ id: `${t}-${i}`, type: t });
  }
  return out;
}

export function emptyPlayedDevCards(): Record<DevelopmentCardType, number> {
  return Object.fromEntries(DEV_ORDER.map((t) => [t, 0])) as Record<
    DevelopmentCardType,
    number
  >;
}

/** Merge API `playedDevelopmentCards` into the played strip shape (unknown keys ignored). */
export function mergePlayedDevCardsFromApi(
  counts: Record<string, number> | undefined | null,
): Record<DevelopmentCardType, number> {
  const base = emptyPlayedDevCards();
  if (!counts || typeof counts !== "object") return base;
  for (const [k, v] of Object.entries(counts)) {
    const t = normalizeDevCardTypeKey(k);
    if (!t) continue;
    base[t] = Math.max(0, Math.floor(Number(v)));
  }
  return base;
}
