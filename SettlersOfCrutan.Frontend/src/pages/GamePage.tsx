import { Canvas } from "@react-three/fiber";
import { OrbitControls } from "@react-three/drei";
import { useMemo } from "react";
import { game } from "../domain/game/gameExample";

function GamePage() {
  const hexRadius = 1;

  const resourceColors: Record<string, string> = useMemo(
    () => ({
      wood: "#2e8b57",
      brick: "#b7410e",
      sheep: "#9acd32",
      wheat: "#f0e68c",
      ore: "#808080",
      desert: "#d2b48c",
      water: "#1e90ff",
    }),
    []
  );

  const cubeToPosition = (q: number, r: number) => {
    // Pointy-top axial layout (q, r). s is implied by q+r+s=0
    const x = hexRadius * (Math.sqrt(3) * q + (Math.sqrt(3) / 2) * r);
    const z = hexRadius * (1.5 * r);
    return { x, z };
  };

  const vertexFromHexCenters = (
    coords: Array<{ q: number; r: number }>
  ): { x: number; z: number } | null => {
    if (!coords || coords.length === 0) return null;
    let sx = 0;
    let sz = 0;
    for (const c of coords) {
      const p = cubeToPosition(c.q, c.r);
      sx += p.x;
      sz += p.z;
    }
    const n = coords.length;
    return { x: sx / n, z: sz / n };
  };

  return (
    <div style={{ width: "100%", height: "calc(100vh - 80px)" }}>
      <Canvas camera={{ position: [0, 5, 8], fov: 50 }}>
        <ambientLight intensity={0.6} />
        <directionalLight position={[5, 10, 5]} intensity={0.8} />

        {/* Hex tiles */}
        {game.board.hexes.map((hex, idx) => {
          const { x, z } = cubeToPosition(hex.coordinate.q, hex.coordinate.r);
          const color = resourceColors[hex.resource] ?? "#cccccc";
          return (
            <group key={`hex-${idx}`} position={[x, 0, z]}>
              <mesh castShadow receiveShadow>
                {/* Hex prism using 6-sided cylinder */}
                <cylinderGeometry args={[hexRadius, hexRadius, 0.2, 6]} />
                <meshStandardMaterial color={color} />
              </mesh>
              {/* Number token disc */}
              <mesh position={[0, 0.15, 0]}>
                <circleGeometry args={[0.25, 32]} />
                <meshStandardMaterial color={"#fff"} />
              </mesh>
            </group>
          );
        })}

        {/* Roads */}
        {game.board.roads.map((road, idx) => {
          if (road.coordinates.length < 2) return null;
          const a = road.coordinates[0];
          const b = road.coordinates[1];
          const pa = cubeToPosition(a.q, a.r);
          const pb = cubeToPosition(b.q, b.r);
          const mx = (pa.x + pb.x) / 2;
          const mz = (pa.z + pb.z) / 2;

          const axis =
            a.r === b.r ? "r" : a.q === b.q ? "q" : a.s === b.s ? "s" : null;

          const angleByAxis = {
            r: Math.PI / 2, // 90°
            q: (7 * Math.PI) / 6, // 210°
            s: (11 * Math.PI) / 6, // 330°
          } as const;

          const angle = axis ? angleByAxis[axis] : 0;

          // Edge length of a regular hex equals its side length, which is hexRadius
          const length = hexRadius * 0.95; // slight inset so it doesn't poke past the edge
          return (
            <mesh
              key={`road-${idx}`}
              position={[mx, 0.2, mz]}
              rotation={[0, angle, 0]}
              castShadow
              receiveShadow
            >
              <boxGeometry args={[length, 0.1, 0.15]} />
              <meshStandardMaterial color={"#654321"} />
            </mesh>
          );
        })}

        {/* Settlements / Cities */}
        {game.board.populationCenters.map((pc, idx) => {
          if (pc.coordinates.length < 1) return null;
          const p = vertexFromHexCenters(
            pc.coordinates.map(({ q, r }) => ({ q, r }))
          );
          if (!p) return null;
          const isCity = pc.type === "city";
          return (
            <mesh
              key={`pc-${idx}`}
              position={[p.x, isCity ? 0.4 : 0.3, p.z]}
              castShadow
            >
              {isCity ? (
                <boxGeometry args={[0.3, 0.3, 0.3]} />
              ) : (
                <sphereGeometry args={[0.2, 16, 16]} />
              )}
              <meshStandardMaterial color={isCity ? "#222" : "#ffcc00"} />
            </mesh>
          );
        })}

        {/* Ports */}
        {game.board.ports.map((port, idx) => {
          if (port.coordinates.length < 1) return null;
          const c = port.coordinates[0];
          const p = cubeToPosition(c.q, c.r);
          return (
            <group key={`port-${idx}`} position={[p.x, 0.05, p.z]}>
              <mesh castShadow receiveShadow>
                <ringGeometry args={[0.25, 0.35, 32]} />
                <meshStandardMaterial color={"#1e90ff"} />
              </mesh>
            </group>
          );
        })}

        <OrbitControls enablePan enableZoom enableRotate />
      </Canvas>
    </div>
  );
}

export default GamePage;
