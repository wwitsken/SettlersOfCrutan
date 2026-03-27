import type { Hex } from "../../domain/game/board";
import { Text } from "@react-three/drei";
import { cubeToPosition } from "./boardMath";

type Props = {
  hex: Hex;
  hexRadius: number;
  color: string;
  hexNumber: number;
};

export function HexTile({ hex, hexRadius, color, hexNumber }: Props) {
  const { x, z } = cubeToPosition(
    hexRadius,
    hex.coordinate.q,
    hex.coordinate.r
  );

  return (
    <group position={[x, 0, z]}>
      <mesh castShadow receiveShadow>
        <cylinderGeometry args={[hexRadius, hexRadius, 0.2, 6]} />
        <meshStandardMaterial color={color} />
      </mesh>
      <mesh position={[0, 0.11, 0]} rotation={[-Math.PI / 2, 0, 0]}>
        <circleGeometry args={[0.25, 32]} />
        <meshStandardMaterial color={"#fff"} />
      </mesh>

      <Text
        position={[0, 0.12, -0.03]}
        rotation={[-Math.PI / 2, 0, 0]}
        fontSize={0.18}
        color={"black"}
        anchorX="center"
        anchorY="middle"
      >
        {String(hexNumber)}
      </Text>
      <Text
        position={[0, 0.121, 0.056]}
        rotation={[-Math.PI / 2, 0, 0]}
        fontSize={0.065}
        color={"#333"}
        anchorX="center"
        anchorY="middle"
        maxWidth={0.46}
        textAlign="center"
      >
        {`${hex.coordinate.q}:${hex.coordinate.r}:${hex.coordinate.s}`}
      </Text>
    </group>
  );
}
