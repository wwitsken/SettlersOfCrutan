import type { Road } from "../../domain/game/board";
import {
  angleByAxis,
  axisFromCoords,
  cubeToPosition,
  midpoint,
} from "./boardMath";

type Props = {
  road: Road;
  hexRadius: number;
};

export function RoadMesh({ road, hexRadius }: Props) {
  if (road.coordinates.length < 2) return null;

  const a = road.coordinates[0];
  const b = road.coordinates[1];

  const pa = cubeToPosition(hexRadius, a.q, a.r);
  const pb = cubeToPosition(hexRadius, b.q, b.r);
  const m = midpoint(pa, pb);

  const axis = axisFromCoords(a, b);
  const angle = axis ? angleByAxis[axis] : 0;

  const length = hexRadius * 0.95;

  return (
    <mesh
      position={[m.x, 0.2, m.z]}
      rotation={[0, angle, 0]}
      castShadow
      receiveShadow
    >
      <boxGeometry args={[length, 0.1, 0.15]} />
      <meshStandardMaterial color={"#654321"} />
    </mesh>
  );
}
