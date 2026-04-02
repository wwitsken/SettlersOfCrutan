import type { PopulationCenter } from "../../../domain/game/board";
import { vertexFromCoords } from "../boardMath";

type Props = {
  populationCenter: PopulationCenter;
  hexRadius: number;
  colorHex?: string;
};

export function CityMesh({
  populationCenter,
  hexRadius,
  colorHex = "#94a3b8",
}: Props) {
  if (populationCenter.coordinates.length < 1) return null;

  const v = vertexFromCoords(
    hexRadius,
    populationCenter.coordinates.map(({ q, r }) => ({ q, r })),
  );
  if (!v) return null;

  return (
    <mesh position={[v.x, 0.25, v.z]} castShadow>
      <boxGeometry args={[0.3, 0.4, 0.3]} />
      <meshStandardMaterial color={colorHex} />
    </mesh>
  );
}
