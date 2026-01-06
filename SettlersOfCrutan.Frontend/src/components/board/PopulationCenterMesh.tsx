import type { PopulationCenter } from "../../domain/game/board";
import { vertexFromCoords } from "./boardMath";

type Props = {
  populationCenter: PopulationCenter;
  hexRadius: number;
};

export function PopulationCenterMesh({ populationCenter, hexRadius }: Props) {
  if (populationCenter.coordinates.length < 1) return null;

  const v = vertexFromCoords(
    hexRadius,
    populationCenter.coordinates.map(({ q, r }) => ({ q, r }))
  );
  if (!v) return null;

  const isCity = populationCenter.type === "city";

  return (
    <mesh position={[v.x, 0.3, v.z]} castShadow>
      {isCity ? (
        <boxGeometry args={[0.3, 0.4, 0.3]} />
      ) : (
        <sphereGeometry args={[0.2, 16, 16]} />
      )}
      <meshStandardMaterial color={isCity ? "#222" : "#ffcc00"} />
    </mesh>
  );
}
