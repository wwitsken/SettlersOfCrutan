import type { PopulationCenter } from "../../domain/game/board";
import { useState } from "react";
import { vertexFromCoords } from "./boardMath";

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

  return (
    <group position={[v.x, 0.3, v.z]}>
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
        {isCity ? (
          <boxGeometry args={[0.3, 0.4, 0.3]} />
        ) : (
          <sphereGeometry args={[0.2, 16, 16]} />
        )}
        <meshStandardMaterial color={colorHex} />
      </mesh>

      {showUpgradeGhost && (
        <mesh position={[0, 0.05, 0]} castShadow>
          <boxGeometry args={[0.3, 0.4, 0.3]} />
          <meshStandardMaterial color={"#ffffff"} transparent opacity={0.5} />
        </mesh>
      )}
    </group>
  );
}
