import * as THREE from "three";

export const SETTLEMENT_S = 0.26;

export function makeRoofGeometry(width: number, depth: number, height: number) {
  const hw = width / 2;
  const hd = depth / 2;
  const v = new Float32Array([
    -hw, 0, -hd,
     hw, 0, -hd,
      0, height, -hd,
    -hw, 0,  hd,
     hw, 0,  hd,
      0, height,  hd,
  ]);
  const idx = [0, 2, 1, 3, 4, 5, 0, 1, 4, 0, 4, 3, 0, 3, 5, 0, 5, 2, 1, 2, 5, 1, 5, 4];
  const geo = new THREE.BufferGeometry();
  geo.setAttribute("position", new THREE.BufferAttribute(v, 3));
  geo.setIndex(idx);
  geo.computeVertexNormals();
  return geo;
}

export const settlementRoofGeometry = makeRoofGeometry(
  SETTLEMENT_S,
  SETTLEMENT_S,
  SETTLEMENT_S * 0.65,
);
