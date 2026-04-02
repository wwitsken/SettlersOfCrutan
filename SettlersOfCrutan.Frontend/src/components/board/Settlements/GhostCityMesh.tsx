export function GhostCityMesh() {
  return (
    <mesh position={[0, 0.05, 0]}>
      <boxGeometry args={[0.3, 0.4, 0.3]} />
      <meshStandardMaterial color="#ffffff" transparent opacity={0.7} />
    </mesh>
  );
}
