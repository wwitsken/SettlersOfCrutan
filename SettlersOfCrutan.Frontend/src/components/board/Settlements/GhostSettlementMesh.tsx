import { useState } from "react";
import * as THREE from "three";

const S = 0.26;
const halfS = S / 2;

function makeRoofGeometry(width: number, depth: number, height: number) {
  const hw = width / 2;
  const hd = depth / 2;
  const v = new Float32Array([
    -hw,
    0,
    -hd,
    hw,
    0,
    -hd,
    0,
    height,
    -hd,
    -hw,
    0,
    hd,
    hw,
    0,
    hd,
    0,
    height,
    hd,
  ]);
  const idx = [
    0, 2, 1, 3, 4, 5, 0, 1, 4, 0, 4, 3, 0, 3, 5, 0, 5, 2, 1, 2, 5, 1, 5, 4,
  ];
  const geo = new THREE.BufferGeometry();
  geo.setAttribute("position", new THREE.BufferAttribute(v, 3));
  geo.setIndex(idx);
  geo.computeVertexNormals();
  return geo;
}

const roofGeometry = makeRoofGeometry(S, S, S * 0.65);

type Props = {
  position: [number, number, number];
  onClick: () => void;
};

export function GhostSettlementMesh({ position, onClick }: Props) {
  const [isHovered, setIsHovered] = useState(false);

  const opacity = isHovered ? 0.7 : 0;

  return (
    <group
      position={position}
      onPointerOver={() => setIsHovered(true)}
      onPointerOut={() => setIsHovered(false)}
      onClick={(e) => {
        e.stopPropagation();
        onClick();
      }}
    >
      <mesh>
        <boxGeometry args={[S, S, S]} />
        <meshStandardMaterial color="#ffffff" transparent opacity={opacity} />
      </mesh>
      <mesh position={[0, halfS, 0]} geometry={roofGeometry}>
        <meshStandardMaterial color="#ffffff" transparent opacity={opacity} />
      </mesh>
    </group>
  );
}
