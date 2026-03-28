import type { HexCoordinate } from "./board";
import type { components } from "../../api/types";

export type HexCoordDto = components["schemas"]["HexCoordDto"];
export type EdgeCoordDto = components["schemas"]["EdgeCoordDto"];
export type VertexCoordDto = components["schemas"]["VertexCoordDto"];

export function parseHexKey(key: string): HexCoordinate {
  const [q, r, s] = key.split(",").map((n) => Number(n.trim()));
  return {
    q,
    r,
    s: Number.isFinite(s) ? s : -q - r,
  };
}

export function hexEq(a: HexCoordinate, b: HexCoordinate): boolean {
  return a.q === b.q && a.r === b.r && a.s === b.s;
}

export function hexKey(h: HexCoordinate): string {
  return `${h.q},${h.r},${h.s}`;
}

export function sortHexes(coords: HexCoordinate[]): HexCoordinate[] {
  return [...coords].sort((a, b) => {
    if (a.q !== b.q) return a.q - b.q;
    if (a.r !== b.r) return a.r - b.r;
    return a.s - b.s;
  });
}

export function edgeKey(a: HexCoordinate, b: HexCoordinate): string {
  const [x, y] = sortHexes([a, b]);
  return `${hexKey(x)}|${hexKey(y)}`;
}

export function vertexKey(coords: HexCoordinate[]): string {
  return sortHexes(coords).map(hexKey).join("|");
}

export function toHexCoordDto(h: HexCoordinate): HexCoordDto {
  return { q: h.q, r: h.r, s: h.s };
}

export function toEdgeCoordDto(a: HexCoordinate, b: HexCoordinate): EdgeCoordDto {
  const [x, y] = sortHexes([a, b]);
  return { hexCoord1: toHexCoordDto(x), hexCoord2: toHexCoordDto(y) };
}

export function toVertexCoordDto(
  h1: HexCoordinate,
  h2: HexCoordinate,
  h3: HexCoordinate,
): VertexCoordDto {
  const [a, b, c] = sortHexes([h1, h2, h3]);
  return {
    hexCoord1: toHexCoordDto(a),
    hexCoord2: toHexCoordDto(b),
    hexCoord3: toHexCoordDto(c),
  };
}

function num(v: number | string | undefined): number {
  if (v === undefined || v === null) return 0;
  return typeof v === "number" ? v : Number(v);
}

export function vertexDtoToHexes(v: VertexCoordDto): HexCoordinate[] {
  return [
    {
      q: num(v.hexCoord1.q),
      r: num(v.hexCoord1.r),
      s: num(v.hexCoord1.s),
    },
    {
      q: num(v.hexCoord2.q),
      r: num(v.hexCoord2.r),
      s: num(v.hexCoord2.s),
    },
    {
      q: num(v.hexCoord3.q),
      r: num(v.hexCoord3.r),
      s: num(v.hexCoord3.s),
    },
  ];
}

/** Both edge endpoints are among the three hexes meeting at a vertex. */
export function edgeTouchesVertex(
  vertexHexes: HexCoordinate[],
  edge: [HexCoordinate, HexCoordinate],
): boolean {
  if (vertexHexes.length < 3) return false;
  return (
    vertexHexes.some((h) => hexEq(h, edge[0])) &&
    vertexHexes.some((h) => hexEq(h, edge[1]))
  );
}

export function rowMatchesEdge(
  row: HexCoordinate[],
  a: HexCoordinate,
  b: HexCoordinate,
): boolean {
  if (row.length !== 2) return false;
  return edgeKey(row[0], row[1]) === edgeKey(a, b);
}

export function rowMatchesVertex(
  row: HexCoordinate[],
  v1: HexCoordinate,
  v2: HexCoordinate,
  v3: HexCoordinate,
): boolean {
  if (row.length !== 3) return false;
  return vertexKey(row) === vertexKey([v1, v2, v3]);
}

export function isEdgeInBuildableList(
  buildable: HexCoordinate[][],
  a: HexCoordinate,
  b: HexCoordinate,
): boolean {
  return buildable.some((row) => rowMatchesEdge(row, a, b));
}

export function isVertexInBuildableList(
  buildable: HexCoordinate[][],
  v1: HexCoordinate,
  v2: HexCoordinate,
  v3: HexCoordinate,
): boolean {
  return buildable.some((row) => rowMatchesVertex(row, v1, v2, v3));
}
