import type {
  DevelopmentCardType,
  PlayerColor,
  ResourceCardType,
} from "../domain/game/gameTypes";

export const COLOR_MAP: Record<PlayerColor, string> = {
  red: "bg-red-500",
  blue: "bg-blue-500",
  orange: "bg-orange-400",
  white: "bg-gray-100 border border-gray-400",
  green: "bg-green-500",
  yellow: "bg-yellow-400",
  brown: "bg-amber-700",
  purple: "bg-purple-500",
  none: "bg-stone-600",
};

export const COLOR_TEXT_MAP: Record<PlayerColor, string> = {
  red: "text-red-400",
  blue: "text-blue-400",
  orange: "text-orange-400",
  white: "text-gray-400",
  green: "text-green-400",
  yellow: "text-yellow-400",
  brown: "text-amber-600",
  purple: "text-purple-400",
  none: "text-stone-500",
};

export const COLOR_BORDER_MAP: Record<PlayerColor, string> = {
  red: "border-red-500",
  blue: "border-blue-500",
  orange: "border-orange-400",
  white: "border-gray-400",
  green: "border-green-500",
  yellow: "border-yellow-400",
  brown: "border-amber-700",
  purple: "border-purple-500",
  none: "border-stone-600",
};

/** Resource types shown in the hand strip (OpenAPI / domain names). */
export const RESOURCE_HAND_TYPES: ResourceCardType[] = [
  "brick",
  "lumber",
  "wool",
  "grain",
  "ore",
];

export const RESOURCE_META: Record<
  ResourceCardType,
  { label: string; emoji: string; bg: string; border: string }
> = {
  none: { label: "None", emoji: "—", bg: "bg-stone-900", border: "border-stone-700" },
  brick: { label: "Brick", emoji: "🧱", bg: "bg-red-950", border: "border-red-800" },
  lumber: { label: "Lumber", emoji: "🌲", bg: "bg-emerald-950", border: "border-emerald-700" },
  wool: { label: "Wool", emoji: "🐑", bg: "bg-lime-950", border: "border-lime-700" },
  grain: { label: "Grain", emoji: "🌾", bg: "bg-yellow-950", border: "border-yellow-700" },
  ore: { label: "Ore", emoji: "⛰️", bg: "bg-slate-800", border: "border-slate-600" },
  desert: { label: "Desert", emoji: "🏜️", bg: "bg-amber-950", border: "border-amber-800" },
  water: { label: "Water", emoji: "💧", bg: "bg-sky-950", border: "border-sky-700" },
};

export const DEV_TYPES_PLAYED_STRIP: DevelopmentCardType[] = [
  "knight",
  "monopoly",
  "roadBuilding",
  "yearOfPlenty",
  "victoryPoint",
];

export const DEV_META: Record<
  DevelopmentCardType,
  { label: string; emoji: string; bg: string; border: string }
> = {
  knight: { label: "Knight", emoji: "⚔️", bg: "bg-purple-950", border: "border-purple-700" },
  monopoly: { label: "Monopoly", emoji: "🏦", bg: "bg-indigo-950", border: "border-indigo-700" },
  roadBuilding: {
    label: "Road Building",
    emoji: "🛣️",
    bg: "bg-amber-950",
    border: "border-amber-700",
  },
  yearOfPlenty: {
    label: "Year of Plenty",
    emoji: "🌟",
    bg: "bg-teal-950",
    border: "border-teal-700",
  },
  victoryPoint: {
    label: "Victory Point",
    emoji: "🏆",
    bg: "bg-rose-950",
    border: "border-rose-700",
  },
};
