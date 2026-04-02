import type { DevelopmentCardType } from "./gameTypes";

const DEV_ORDER: DevelopmentCardType[] = [
  "knight",
  "monopoly",
  "roadBuilding",
  "yearOfPlenty",
  "victoryPoint",
];

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
