import * as THREE from "three";

export type CubeCoord = { q: number; r: number; s: number };

export type WorldXZ = { x: number; z: number };

export const angleByAxis = {
  r: Math.PI / 2, // 90°
  q: (7 * Math.PI) / 6, // 210°
  s: (11 * Math.PI) / 6, // 330°
} as const;

export type Axis = keyof typeof angleByAxis;

export function axisFromCoords(a: CubeCoord, b: CubeCoord): Axis | null {
  return a.r === b.r ? "r" : a.q === b.q ? "q" : a.s === b.s ? "s" : null;
}

// Pointy-top axial layout (q, r). s is implied by q+r+s=0
export function cubeToPosition(
  hexRadius: number,
  q: number,
  r: number
): WorldXZ {
  const x = hexRadius * (Math.sqrt(3) * q + (Math.sqrt(3) / 2) * r);
  const z = hexRadius * (1.5 * r);
  return { x, z };
}

export function midpoint(a: WorldXZ, b: WorldXZ): WorldXZ {
  return { x: (a.x + b.x) / 2, z: (a.z + b.z) / 2 };
}

export function vertexFromCoords(
  hexRadius: number,
  coords: Array<{ q: number; r: number }>
): WorldXZ | null {
  if (!coords || coords.length === 0) return null;
  let sx = 0;
  let sz = 0;
  for (const c of coords) {
    const p = cubeToPosition(hexRadius, c.q, c.r);
    sx += p.x;
    sz += p.z;
  }
  const n = coords.length;
  return { x: sx / n, z: sz / n };
}

export function vertexFromHexCenters(centers: WorldXZ[]): WorldXZ | null {
  if (!centers || centers.length === 0) return null;
  let sx = 0;
  let sz = 0;
  for (const p of centers) {
    sx += p.x;
    sz += p.z;
  }
  const n = centers.length;
  return { x: sx / n, z: sz / n };
}

export function createPortShape(
  outerRadius: number,
  innerRadius: number
): THREE.Shape {
  // Semi-annulus in XY plane centered at origin.
  // Arc is on +X side; flat cut is along the Y axis.
  const shape = new THREE.Shape();
  shape.absarc(0, 0, outerRadius, -Math.PI / 2, Math.PI / 2, false);
  shape.lineTo(0, innerRadius);

  const hole = new THREE.Path();
  hole.absarc(0, 0, innerRadius, Math.PI / 2, -Math.PI / 2, true);
  shape.holes.push(hole);

  return shape;
}
