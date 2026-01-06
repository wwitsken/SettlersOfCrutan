import * as THREE from "three";
import { useMemo } from "react";
import type { Port } from "../../domain/game/board";
import {
  angleByAxis,
  axisFromCoords,
  cubeToPosition,
  createPortShape,
  midpoint,
} from "./boardMath";

type Props = {
  port: Port;
  hexRadius: number;
};

export function PortMesh({ port, hexRadius }: Props) {
  const portOuterRadius = 0.35;
  const portInnerRadius = 0.25;
  const portHeight = 0.03;

  const a = port.inCoordinate;
  const b = port.outCoordinate;

  const pa = cubeToPosition(hexRadius, a.q, a.r);
  const pb = cubeToPosition(hexRadius, b.q, b.r);
  const m = midpoint(pa, pb);

  const axis = axisFromCoords(a, b);
  const baseAngle = axis ? angleByAxis[axis] : 0;

  // Align the flat edge of the semicircle to the same edge direction as roads.
  const yaw = baseAngle + Math.PI / 2;

  const shape = useMemo(
    () => createPortShape(portOuterRadius, portInnerRadius),
    [portInnerRadius, portOuterRadius]
  );
  const extrudeSettings = useMemo<THREE.ExtrudeGeometryOptions>(
    () => ({ depth: portHeight, bevelEnabled: false, steps: 1 }),
    []
  );

  return (
    <group position={[m.x, 0.06, m.z]} rotation={[0, yaw, 0]}>
      <mesh castShadow receiveShadow rotation={[-Math.PI / 2, 0, Math.PI]}>
        <extrudeGeometry args={[shape, extrudeSettings]} />
        <meshStandardMaterial color={"#1e90ff"} />
      </mesh>
    </group>
  );
}
