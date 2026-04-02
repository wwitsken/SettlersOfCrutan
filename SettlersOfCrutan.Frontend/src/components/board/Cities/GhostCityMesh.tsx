import { useState } from "react";

type Props = {
  position: [number, number, number];
  onClick: () => void;
};

export function GhostCityMesh({ position, onClick }: Props) {
  const [isHovered, setIsHovered] = useState(false);

  const opacity = isHovered ? 0.65 : 0;

  return (
    <mesh
      position={position}
      castShadow
      onPointerOver={() => setIsHovered(true)}
      onPointerOut={() => setIsHovered(false)}
      onClick={(e) => {
        e.stopPropagation();
        onClick();
      }}
    >
      <boxGeometry args={[0.3, 0.4, 0.3]} />
      <meshStandardMaterial color="#ffffff" transparent opacity={opacity} />
    </mesh>
  );
}
