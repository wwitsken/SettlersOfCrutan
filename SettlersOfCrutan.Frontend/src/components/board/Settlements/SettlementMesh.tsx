import type { PopulationCenter } from "../../../domain/game/board";
import { useState } from "react";
import { vertexFromCoords } from "../boardMath";
import { GhostCityMesh } from "../Cities/GhostCityMesh";
import { SETTLEMENT_S, settlementRoofGeometry } from "./settlementGeometry";

type Props = {
  populationCenter: PopulationCenter;
  hexRadius: number;
  /** Settlement body color. */
  colorHex?: string;
  /** When true, hovering shows a city ghost; click settlement or ghost upgrades. */
  upgradeable?: boolean;
  onUpgrade?: (populationCenter: PopulationCenter) => void;
};

export function SettlementMesh({
  populationCenter,
  hexRadius,
  colorHex = "#94a3b8",
  upgradeable = false,
  onUpgrade,
}: Props) {
  const [isHovered, setIsHovered] = useState(false);

  if (populationCenter.coordinates.length < 1) return null;

  const v = vertexFromCoords(
    hexRadius,
    populationCenter.coordinates.map(({ q, r }) => ({ q, r })),
  );
  if (!v) return null;

  const halfS = SETTLEMENT_S / 2;

  return (
    <group
      position={[v.x, 0.25, v.z]}
      onPointerOver={() => setIsHovered(true)}
      onPointerOut={() => setIsHovered(false)}
      onClick={(e) => {
        if (upgradeable && onUpgrade) {
          e.stopPropagation();
          onUpgrade(populationCenter);
        }
      }}
    >
      <mesh position={[0, 0, 0]} castShadow>
        <boxGeometry args={[SETTLEMENT_S, SETTLEMENT_S, SETTLEMENT_S]} />
        <meshStandardMaterial color={colorHex} />
      </mesh>
      <mesh
        position={[0, halfS, 0]}
        castShadow
        geometry={settlementRoofGeometry}
      >
        <meshStandardMaterial color={colorHex} />
      </mesh>

      {upgradeable && isHovered && (
        <group position={[0, halfS + 0.2, 0]}>
          <GhostCityMesh
            position={[0, 0, 0]}
            onClick={() => {
              if (upgradeable && onUpgrade) onUpgrade(populationCenter);
            }}
          />
        </group>
      )}
    </group>
  );
}
