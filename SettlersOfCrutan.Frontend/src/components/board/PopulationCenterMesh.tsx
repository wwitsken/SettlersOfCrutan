import type { PopulationCenter } from "../../domain/game/board";
import { useMemo, useState } from "react";
import * as THREE from "three";
import { vertexFromCoords } from "./boardMath";

/** House footprint cube size (world units); roof sits flush on top. */
const SETTLEMENT_S = 0.26;

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

const settlementRoofGeometry = makeRoofGeometry(
  SETTLEMENT_S,
  SETTLEMENT_S,
  SETTLEMENT_S * 0.65,
);

type Props = {
  populationCenter: PopulationCenter;
  hexRadius: number;
  /** Settlement / city body color. */
  colorHex?: string;
  enableCityUpgradeHover?: boolean;
  selectForCityUpgrade?: boolean;
  onCityUpgradeSelect?: (populationCenter: PopulationCenter) => void;
};

export function PopulationCenterMesh({
  populationCenter,
  hexRadius,
  colorHex = "#94a3b8",
  enableCityUpgradeHover = false,
  selectForCityUpgrade = false,
  onCityUpgradeSelect,
}: Props) {
  const [isHovered, setIsHovered] = useState(false);

  if (populationCenter.coordinates.length < 1) return null;

  const v = vertexFromCoords(
    hexRadius,
    populationCenter.coordinates.map(({ q, r }) => ({ q, r }))
  );
  if (!v) return null;

  const isCity = populationCenter.type === "city";
  const showUpgradeGhost = enableCityUpgradeHover && !isCity && isHovered;
  const canClickCity =
    selectForCityUpgrade && !isCity && onCityUpgradeSelect;

  const halfS = SETTLEMENT_S / 2;

  return (
    <group position={[v.x, 0.25, v.z]}>
      {isCity ? (
        <mesh
          castShadow
          onClick={(e) => {
            if (canClickCity) {
              e.stopPropagation();
              onCityUpgradeSelect(populationCenter);
            }
          }}
          onPointerOver={() => setIsHovered(true)}
          onPointerOut={() => setIsHovered(false)}
        >
          <boxGeometry args={[0.3, 0.4, 0.3]} />
          <meshStandardMaterial color={colorHex} />
        </mesh>
      ) : (
        <group
          onClick={(e) => {
            if (canClickCity) {
              e.stopPropagation();
              onCityUpgradeSelect(populationCenter);
            }
          }}
          onPointerOver={() => setIsHovered(true)}
          onPointerOut={() => setIsHovered(false)}
        >
          <mesh position={[0, 0, 0]} castShadow>
            <boxGeometry
              args={[SETTLEMENT_S, SETTLEMENT_S, SETTLEMENT_S]}
            />
            <meshStandardMaterial color={colorHex} />
          </mesh>
          <mesh
            position={[0, halfS, 0]}
            castShadow
            geometry={settlementRoofGeometry}
          >
            <meshStandardMaterial color={colorHex} />
          </mesh>
        </group>
      )}

      {showUpgradeGhost && (
        <mesh position={[0, 0.05, 0]} castShadow>
          <boxGeometry args={[0.3, 0.4, 0.3]} />
          <meshStandardMaterial color={"#ffffff"} transparent opacity={0.5} />
        </mesh>
      )}
    </group>
  );
}
