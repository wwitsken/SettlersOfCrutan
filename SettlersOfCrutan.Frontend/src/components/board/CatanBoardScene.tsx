import { useMemo, useState } from "react";
import { Canvas } from "@react-three/fiber";
import { OrbitControls } from "@react-three/drei";
import type { Game } from "../../domain/game/game";
import type { ResourceCardType } from "../../domain/game/gameTypes";
import { HexTile } from "./HexTile";
import { RoadMesh } from "./RoadMesh";
import { PopulationCenterMesh } from "./PopulationCenterMesh";
import { PortMesh } from "./PortMesh";
import { axisFromCoords, cubeToPosition, midpoint } from "./boardMath";

type HoverMode = "none" | "road" | "settlement" | "cityUpgrade";

type Props = {
  game: Game;
  hexRadius?: number;
  hoverMode?: HoverMode;
};

export function CatanBoardScene({
  game,
  hexRadius = 1,
  hoverMode = "road",
}: Props) {
  const board = game.board;

  const [hoveredRoadKey, setHoveredRoadKey] = useState<string | null>(null);
  const [hoveredVertexKey, setHoveredVertexKey] = useState<string | null>(null);

  const resourceColors: Record<ResourceCardType, string> = {
    none: "#94a3b8",
    brick: "#b7410e",
    lumber: "#2e8b57",
    wool: "#9acd32",
    grain: "#f0e68c",
    ore: "#808080",
    desert: "#d2b48c",
    water: "#1e90ff",
  };

  const hoverRoadCandidates = useMemo(() => {
    if (hoverMode !== "road") return [] as const;

    const directions = [
      { q: 1, r: -1, s: 0 },
      { q: 1, r: 0, s: -1 },
      { q: 0, r: 1, s: -1 },
      { q: -1, r: 1, s: 0 },
      { q: -1, r: 0, s: 1 },
      { q: 0, r: -1, s: 1 },
    ] as const;

    const dedup = new Map<
      string,
      { key: string; x: number; z: number; angle: number }
    >();

    for (const hex of board.hexes) {
      for (const d of directions) {
        const neighborKey = `${hex.coordinate.q + d.q},${
          hex.coordinate.r + d.r
        },${hex.coordinate.s + d.s}`;
        const neighborCoord = {
          q: hex.coordinate.q + d.q,
          r: hex.coordinate.r + d.r,
          s: hex.coordinate.s + d.s,
        };

        const aKey = `${hex.coordinate.q},${hex.coordinate.r},${hex.coordinate.s}`;
        const bKey = neighborKey;
        const key = aKey < bKey ? `${aKey}|${bKey}` : `${bKey}|${aKey}`;
        if (dedup.has(key)) continue;

        const pa = cubeToPosition(
          hexRadius,
          hex.coordinate.q,
          hex.coordinate.r
        );
        const pb = cubeToPosition(hexRadius, neighborCoord.q, neighborCoord.r);
        const m = midpoint(pa, pb);

        const axis = axisFromCoords(hex.coordinate, neighborCoord);
        const angle =
          axis === "r"
            ? Math.PI / 2
            : axis === "q"
            ? (7 * Math.PI) / 6
            : axis === "s"
            ? (11 * Math.PI) / 6
            : 0;

        dedup.set(key, { key, x: m.x, z: m.z, angle });
      }
    }

    return Array.from(dedup.values());
  }, [board.hexes, hexRadius, hoverMode]);

  const hoverVertexCandidates = useMemo(() => {
    if (hoverMode !== "settlement") return [] as const;

    const directions = [
      { q: 1, r: -1, s: 0 },
      { q: 1, r: 0, s: -1 },
      { q: 0, r: 1, s: -1 },
      { q: -1, r: 1, s: 0 },
      { q: -1, r: 0, s: 1 },
      { q: 0, r: -1, s: 1 },
    ] as const;

    const dedup = new Map<string, { key: string; x: number; z: number }>();

    for (const hex of board.hexes) {
      const baseKey = `${hex.coordinate.q},${hex.coordinate.r},${hex.coordinate.s}`;
      const p0 = cubeToPosition(hexRadius, hex.coordinate.q, hex.coordinate.r);

      for (let i = 0; i < 6; i++) {
        const d1 = directions[i];
        const d2 = directions[(i + 5) % 6];

        const n1Key = `${hex.coordinate.q + d1.q},${hex.coordinate.r + d1.r},${
          hex.coordinate.s + d1.s
        }`;
        const n2Key = `${hex.coordinate.q + d2.q},${hex.coordinate.r + d2.r},${
          hex.coordinate.s + d2.s
        }`;

        const p1 = cubeToPosition(
          hexRadius,
          hex.coordinate.q + d1.q,
          hex.coordinate.r + d1.r
        );
        const p2 = cubeToPosition(
          hexRadius,
          hex.coordinate.q + d2.q,
          hex.coordinate.r + d2.r
        );

        const vx = (p0.x + p1.x + p2.x) / 3;
        const vz = (p0.z + p1.z + p2.z) / 3;

        // floating-safe dedupe key
        const kx = Math.round(vx * 1000);
        const kz = Math.round(vz * 1000);
        const vKey = `${kx},${kz}`;

        // include the source triple for stability/debuggability if needed
        const tripleKey = [baseKey, n1Key, n2Key].sort().join("|");
        const key = `${vKey}|${tripleKey}`;
        if (!dedup.has(vKey)) {
          dedup.set(vKey, { key, x: vx, z: vz });
        }
      }
    }

    return Array.from(dedup.values());
  }, [board.hexes, hexRadius, hoverMode]);

  return (
    <div style={{ width: "100%", height: "100%" }}>
      <Canvas camera={{ position: [0, 5, 8], fov: 50 }}>
        <ambientLight intensity={0.6} />
        <directionalLight position={[5, 10, 5]} intensity={0.8} />
        <OrbitControls enablePan enableZoom enableRotate />

        <group>
          {board.hexes.map((hex, idx) => (
            <HexTile
              key={`${hex.coordinate.q},${hex.coordinate.r},${hex.coordinate.s}-${idx}`}
              hex={hex}
              hexRadius={hexRadius}
              color={resourceColors[hex.resource] ?? "#cccccc"}
              hexNumber={hex.numberToken}
            />
          ))}

          {hoverMode === "road" && (
            <group>
              {hoverRoadCandidates.map((c) => (
                <group
                  key={c.key}
                  position={[c.x, 0.2, c.z]}
                  rotation={[0, c.angle, 0]}
                >
                  <mesh
                    onPointerOver={() => setHoveredRoadKey(c.key)}
                    onPointerOut={() =>
                      setHoveredRoadKey((prev) =>
                        prev === c.key ? null : prev
                      )
                    }
                  >
                    <boxGeometry args={[hexRadius * 0.95, 0.1, 0.15]} />
                    <meshStandardMaterial transparent opacity={0} />
                  </mesh>

                  {hoveredRoadKey === c.key && (
                    <mesh castShadow>
                      <boxGeometry args={[hexRadius * 0.95, 0.1, 0.15]} />
                      <meshStandardMaterial
                        color={"#ffffff"}
                        transparent
                        opacity={0.5}
                      />
                    </mesh>
                  )}
                </group>
              ))}
            </group>
          )}

          {hoverMode === "settlement" && (
            <group>
              {hoverVertexCandidates.map((v) => (
                <group key={v.key} position={[v.x, 0.3, v.z]}>
                  <mesh
                    onPointerOver={() => setHoveredVertexKey(v.key)}
                    onPointerOut={() =>
                      setHoveredVertexKey((prev) =>
                        prev === v.key ? null : prev
                      )
                    }
                  >
                    <sphereGeometry args={[0.25, 12, 12]} />
                    <meshStandardMaterial transparent opacity={0} />
                  </mesh>

                  {hoveredVertexKey === v.key && (
                    <mesh castShadow>
                      <sphereGeometry args={[0.2, 16, 16]} />
                      <meshStandardMaterial
                        color={"#ffffff"}
                        transparent
                        opacity={0.5}
                      />
                    </mesh>
                  )}
                </group>
              ))}
            </group>
          )}

          {board.roads.map((road, idx) => (
            <RoadMesh key={`road-${idx}`} road={road} hexRadius={hexRadius} />
          ))}

          {board.populationCenters.map((pc, idx) => (
            <PopulationCenterMesh
              key={`pc-${idx}`}
              populationCenter={pc}
              hexRadius={hexRadius}
              enableCityUpgradeHover={hoverMode === "cityUpgrade"}
            />
          ))}

          {board.ports.map((port, idx) => (
            <PortMesh key={`port-${idx}`} port={port} hexRadius={hexRadius} />
          ))}
        </group>
      </Canvas>
    </div>
  );
}
