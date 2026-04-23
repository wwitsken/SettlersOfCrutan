import { useState } from "react";
import { SETTLEMENT_S, settlementRoofGeometry } from "./settlementGeometry";

const halfS = SETTLEMENT_S / 2;

type Props = {
  position: [number, number, number];
  onClick?: () => void;
  /**
   * When true, the ghost is rendered at a persistent low opacity regardless of
   * hover state. Used to mark an already-committed-but-not-finalized
   * placement (e.g. the first half of the setup-phase settlement+road pair).
   * Hover is still honored to give extra feedback on top of the baseline.
   */
  alwaysVisible?: boolean;
  /** Optional tint (e.g. the placing player's color). Defaults to white. */
  color?: string;
};

export function GhostSettlementMesh({
  position,
  onClick,
  alwaysVisible = false,
  color = "#ffffff",
}: Props) {
  const [isHovered, setIsHovered] = useState(false);

  const baseOpacity = alwaysVisible ? 0.55 : 0;
  const opacity = isHovered ? 0.85 : baseOpacity;

  return (
    <group
      position={position}
      onPointerOver={() => setIsHovered(true)}
      onPointerOut={() => setIsHovered(false)}
      onClick={
        onClick
          ? (e) => {
              e.stopPropagation();
              onClick();
            }
          : undefined
      }
    >
      <mesh>
        <boxGeometry args={[SETTLEMENT_S, SETTLEMENT_S, SETTLEMENT_S]} />
        <meshStandardMaterial color={color} transparent opacity={opacity} />
      </mesh>
      <mesh position={[0, halfS, 0]} geometry={settlementRoofGeometry}>
        <meshStandardMaterial color={color} transparent opacity={opacity} />
      </mesh>
    </group>
  );
}
