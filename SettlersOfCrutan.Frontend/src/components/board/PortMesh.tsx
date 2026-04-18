import * as THREE from "three";
import { Billboard, Text } from "@react-three/drei";
import { useMemo } from "react";
import type { Port } from "../../domain/game/board";
import { RESOURCE_META } from "../../constants/catanMeta";
import { cubeToPosition, createPortShape, midpoint } from "./boardMath";

type Props = {
  port: Port;
  hexRadius: number;
};

/** API / domain JSON uses camelCase (e.g. generic3to1, brick2to1). */
function normalizePortTypeKey(raw: string): string {
  const t = raw.trim();
  if (!t) return "generic3to1";
  return t.charAt(0).toLowerCase() + t.slice(1);
}

type PortSkin = {
  ratioLine: string;
  detailLine: string;
  /** Pier / planking */
  dock: string;
  /** Trade marker disc */
  token: string;
  tokenEmissive: string;
};

const PORT_SKINS: Record<string, PortSkin> = {
  generic3to1: {
    ratioLine: "3:1",
    detailLine: "❓ ANY",
    dock: "#5c4a3a",
    token: "#475569",
    tokenEmissive: "#1e293b",
  },
  brick2to1: {
    ratioLine: "2:1",
    detailLine: `${RESOURCE_META.brick.emoji} BRICK`,
    dock: "#4a3a32",
    token: "#a85c4a",
    tokenEmissive: "#4a1510",
  },
  lumber2to1: {
    ratioLine: "2:1",
    detailLine: `${RESOURCE_META.lumber.emoji} LUMBER`,
    dock: "#3d4538",
    token: "#3d7a4e",
    tokenEmissive: "#0f2918",
  },
  wool2to1: {
    ratioLine: "2:1",
    detailLine: `${RESOURCE_META.wool.emoji} WOOL`,
    dock: "#454838",
    token: "#7cb342",
    tokenEmissive: "#2a4a12",
  },
  grain2to1: {
    ratioLine: "2:1",
    detailLine: `${RESOURCE_META.grain.emoji} GRAIN`,
    dock: "#524a36",
    token: "#d4a84b",
    tokenEmissive: "#5c4010",
  },
  ore2to1: {
    ratioLine: "2:1",
    detailLine: `${RESOURCE_META.ore.emoji} ORE`,
    dock: "#3a3d42",
    token: "#7d8a9a",
    tokenEmissive: "#1e2430",
  },
  none: {
    ratioLine: "—",
    detailLine: "⚓ PORT",
    dock: "#4a4540",
    token: "#57534e",
    tokenEmissive: "#1c1917",
  },
};

function skinForPortType(type: string): PortSkin {
  const key = normalizePortTypeKey(type);
  return PORT_SKINS[key] ?? PORT_SKINS.generic3to1;
}

export function PortMesh({ port, hexRadius }: Props) {
  const portOuterRadius = 0.38;
  const portInnerRadius = 0.22;
  const portHeight = 0.045;

  const skin = useMemo(() => skinForPortType(port.type), [port.type]);

  const a = port.inCoordinate;
  const b = port.outCoordinate;

  const pa = cubeToPosition(hexRadius, a.q, a.r);
  const pb = cubeToPosition(hexRadius, b.q, b.r);
  const m = midpoint(pa, pb);

  const sea = new THREE.Vector3(pb.x - pa.x, 0, pb.z - pa.z);
  if (sea.lengthSq() < 1e-10) return null;
  sea.normalize();

  const yaw = Math.atan2(-sea.z, sea.x);

  const shape = useMemo(
    () => createPortShape(portOuterRadius, portInnerRadius),
    [portInnerRadius, portOuterRadius],
  );
  const extrudeSettings = useMemo<THREE.ExtrudeGeometryOptions>(
    () => ({ depth: portHeight, bevelEnabled: false, steps: 1 }),
    [portHeight],
  );

  // Marker sits on the seaward bulge of the pier (local +X after yaw).
  const tokenOffsetX = portOuterRadius * 0.46;
  const tokenY = portHeight * 0.55 + 0.012;

  return (
    <group position={[m.x, 0.06, m.z]} rotation={[0, yaw, 0]}>
      {/* Curved coastal pier — wood planking */}
      <mesh castShadow receiveShadow rotation={[-Math.PI / 2, 0, 0]}>
        <extrudeGeometry args={[shape, extrudeSettings]} />
        <meshStandardMaterial
          color={skin.dock}
          roughness={0.88}
          metalness={0.04}
        />
      </mesh>

      {/* Mooring posts */}
      {[
        [-portInnerRadius * 0.35, portHeight + 0.01, portInnerRadius * 0.25],
        [-portInnerRadius * 0.35, portHeight + 0.01, -portInnerRadius * 0.25],
      ].map((pos, i) => (
        <mesh key={i} castShadow position={pos as [number, number, number]}>
          <cylinderGeometry args={[0.028, 0.022, 0.07, 8]} />
          <meshStandardMaterial color="#3d3028" roughness={0.9} />
        </mesh>
      ))}

      {/* Colored trade token — reads like the board’s resource chips */}
      <mesh
        castShadow
        receiveShadow
        position={[tokenOffsetX, tokenY, 0]}
        rotation={[-Math.PI / 2, 0, 0]}
      >
        <circleGeometry args={[0.1, 28]} />
        <meshStandardMaterial
          color={skin.token}
          roughness={0.45}
          metalness={0.12}
          emissive={skin.tokenEmissive}
          emissiveIntensity={0.35}
        />
      </mesh>
      <mesh position={[tokenOffsetX, tokenY + 0.001, 0]} rotation={[-Math.PI / 2, 0, 0]}>
        <ringGeometry args={[0.072, 0.098, 28]} />
        <meshStandardMaterial
          color="#1a1410"
          roughness={0.6}
          metalness={0.25}
        />
      </mesh>

      {/* Trade text — billboarded so it stays readable while orbiting */}
      <Billboard follow position={[tokenOffsetX * 0.2, portHeight + 0.55, 0]}>
        <Text
          fontSize={0.118}
          color="#faf6ed"
          anchorX="center"
          anchorY="middle"
          position={[0, 0.075, 0]}
          outlineWidth={0.028}
          outlineColor="#0c0a08"
          maxWidth={1.1}
          textAlign="center"
        >
          {skin.ratioLine}
        </Text>
        <Text
          fontSize={0.092}
          color="#efe6d4"
          anchorX="center"
          anchorY="middle"
          position={[0, -0.065, 0]}
          outlineWidth={0.022}
          outlineColor="#0c0a08"
          maxWidth={1.2}
          textAlign="center"
          letterSpacing={0.02}
        >
          {skin.detailLine}
        </Text>
      </Billboard>
    </group>
  );
}
