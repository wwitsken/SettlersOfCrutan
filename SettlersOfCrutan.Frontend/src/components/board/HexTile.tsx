import type { Hex } from "../../domain/game/board";
import { cubeToPosition } from "./boardMath";

type Props = {
  hex: Hex;
  hexRadius: number;
  color: string;
};

export function HexTile({ hex, hexRadius, color }: Props) {
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
    </group>
  );
}
