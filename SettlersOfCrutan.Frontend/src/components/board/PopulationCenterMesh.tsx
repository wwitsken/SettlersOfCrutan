import type { PopulationCenter } from "../../domain/game/board";
import { useState } from "react";
import { vertexFromCoords } from "./boardMath";

type Props = {
  populationCenter: PopulationCenter;
  hexRadius: number;
  enableCityUpgradeHover?: boolean;
};

export function PopulationCenterMesh({
  populationCenter,
  hexRadius,
  enableCityUpgradeHover = false,
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

  return (
    <group position={[v.x, 0.3, v.z]}>
      <mesh
        castShadow
        onPointerOver={() => setIsHovered(true)}
        onPointerOut={() => setIsHovered(false)}
      >
        {isCity ? (
          <boxGeometry args={[0.3, 0.4, 0.3]} />
        ) : (
          <sphereGeometry args={[0.2, 16, 16]} />
        )}
        <meshStandardMaterial color={isCity ? "#222" : "#ffcc00"} />
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
