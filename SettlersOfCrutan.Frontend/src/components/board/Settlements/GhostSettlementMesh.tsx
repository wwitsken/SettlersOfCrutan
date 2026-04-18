import { useState } from "react";
import { SETTLEMENT_S, settlementRoofGeometry } from "./settlementGeometry";

const halfS = SETTLEMENT_S / 2;

type Props = {
  position: [number, number, number];
  onClick: () => void;
};

export function GhostSettlementMesh({ position, onClick }: Props) {
  const [isHovered, setIsHovered] = useState(false);

  const opacity = isHovered ? 0.7 : 0;

  return (
    <group
      position={position}
      onPointerOver={() => setIsHovered(true)}
      onPointerOut={() => setIsHovered(false)}
      onClick={(e) => {
        e.stopPropagation();
        onClick();
      }}
    >
      <mesh>
        <boxGeometry args={[SETTLEMENT_S, SETTLEMENT_S, SETTLEMENT_S]} />
        <meshStandardMaterial color="#ffffff" transparent opacity={opacity} />
      </mesh>
      <mesh position={[0, halfS, 0]} geometry={settlementRoofGeometry}>
        <meshStandardMaterial color="#ffffff" transparent opacity={opacity} />
      </mesh>
    </group>
  );
}
