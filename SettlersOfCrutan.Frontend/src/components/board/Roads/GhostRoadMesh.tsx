import { useState } from "react";

type Props = {
  position: [number, number, number];
  angle: number;
  hexRadius: number;
  onClick: () => void;
};

export function GhostRoadMesh({ position, angle, hexRadius, onClick }: Props) {
  const [isHovered, setIsHovered] = useState(false);

  return (
    <mesh
      position={position}
      rotation={[0, angle, 0]}
      onPointerOver={() => setIsHovered(true)}
      onPointerOut={() => setIsHovered(false)}
      onClick={(e) => {
        e.stopPropagation();
        onClick();
      }}
    >
      <boxGeometry args={[hexRadius * 0.95, 0.1, 0.15]} />
      <meshStandardMaterial
        color="#ffffff"
        transparent
        opacity={isHovered ? 0.7 : 0}
      />
    </mesh>
  );
}
