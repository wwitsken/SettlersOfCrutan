import * as THREE from "three";
import { Text } from "@react-three/drei";
import { useMemo } from "react";
import type { Port } from "../../domain/game/board";
import { cubeToPosition, createPortShape, midpoint } from "./boardMath";

type Props = {
  port: Port;
  hexRadius: number;
};

export function PortMesh({ port, hexRadius }: Props) {
  const portOuterRadius = 0.35;
  const portInnerRadius = 0.25;
  const portHeight = 0.03;

  // API: in = land, out = water or off-board (GameMappingExtensions.GetPortInOutCoordinates).
  const a = port.inCoordinate;
  const b = port.outCoordinate;

  const pa = cubeToPosition(hexRadius, a.q, a.r);
  const pb = cubeToPosition(hexRadius, b.q, b.r);
  const m = midpoint(pa, pb);

  // Axis-only angles (r/q/s) cannot distinguish the two opposite directions along an edge,
  // so half the ports were flipped 180°. Bulge toward water using center-to-center seaward vector.
  const sea = new THREE.Vector3(pb.x - pa.x, 0, pb.z - pa.z);
  if (sea.lengthSq() < 1e-10) return null;
  sea.normalize();

  // createPortShape puts the arc on +X; for rotation.y = θ, local +X → (cos θ, 0, −sin θ) in world XZ.
  const yaw = Math.atan2(-sea.z, sea.x);

  const shape = useMemo(
    () => createPortShape(portOuterRadius, portInnerRadius),
    [portInnerRadius, portOuterRadius]
  );
  const extrudeSettings = useMemo<THREE.ExtrudeGeometryOptions>(
    () => ({ depth: portHeight, bevelEnabled: false, steps: 1 }),
    []
  );

  return (
    <>
      <group position={[m.x, 0.07, m.z]} rotation={[0, yaw, 0]}>
        <mesh castShadow receiveShadow rotation={[-Math.PI / 2, 0, 0]}>
          <extrudeGeometry args={[shape, extrudeSettings]} />
          <meshStandardMaterial color={"#1e90ff"} />
        </mesh>
      </group>
      <Text
        position={[m.x, 0.32, m.z]}
        rotation={[-Math.PI / 2, 0, 0]}
        fontSize={0.065}
        color="#0d1f33"
        anchorX="center"
        anchorY="middle"
        maxWidth={0.55}
        textAlign="center"
      >
        {`in ${a.q}:${a.r}:${a.s}\nout ${b.q}:${b.r}:${b.s}`}
      </Text>
    </>
  );
}
