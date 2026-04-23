import { useMemo } from "react";
import { Canvas } from "@react-three/fiber";
import { OrbitControls } from "@react-three/drei";
import type { Game } from "../../domain/game/game";
import type { HexCoordinate, PopulationCenter } from "../../domain/game/board";
import type { ResourceCardType } from "../../domain/game/gameTypes";
import type { BoardPickMode } from "../../domain/game/boardInteraction";
import type { components } from "../../api/types";
import {
  edgeKey,
  edgeTouchesVertex,
  isEdgeInBuildableList,
  isVertexInBuildableList,
  toEdgeCoordDto,
  toVertexCoordDto,
  vertexKey,
} from "../../domain/game/hexCoords";
import { HexTile } from "./HexTile";
import { RoadMesh } from "./Roads/RoadMesh";
import { GhostRoadMesh } from "./Roads/GhostRoadMesh";
import { SettlementMesh } from "./Settlements/SettlementMesh";
import { CityMesh } from "./Cities/CityMesh";
import { GhostSettlementMesh } from "./Settlements/GhostSettlementMesh";
import { PortMesh } from "./PortMesh";
import { axisFromCoords, cubeToPosition, midpoint } from "./boardMath";
import { playerColorToHex } from "../../domain/game/playerColorHex";

type EdgeCoordDto = components["schemas"]["EdgeCoordDto"];
type VertexCoordDto = components["schemas"]["VertexCoordDto"];

type RoadCandidate = {
  key: string;
  x: number;
  z: number;
  angle: number;
  hexA: HexCoordinate;
  hexB: HexCoordinate;
};

type VertexCandidate = {
  key: string;
  x: number;
  z: number;
  h0: HexCoordinate;
  h1: HexCoordinate;
  h2: HexCoordinate;
};

type Props = {
  game: Game;
  hexRadius?: number;
  boardPickMode: BoardPickMode;
  showGhostRoads?: boolean;
  showGhostSettlementVertices?: boolean;
  showGhostCityUpgrades?: boolean;
  buildableRoads?: HexCoordinate[][];
  buildableSettlements?: HexCoordinate[][];
  initialRoadVertexHexes?: HexCoordinate[] | null;
  myPlayerId?: string;
  onRoadPicked?: (edge: EdgeCoordDto) => void;
  onVertexPicked?: (vertex: VertexCoordDto) => void;
  onRobberHexPicked?: (hex: HexCoordinate) => void;
  onSettlementCityPicked?: (vertex: VertexCoordDto) => void;
};

export function CatanBoardScene({
  game,
  hexRadius = 1,
  boardPickMode,
  showGhostRoads = true,
  showGhostSettlementVertices = true,
  showGhostCityUpgrades = false,
  buildableRoads,
  buildableSettlements,
  initialRoadVertexHexes,
  myPlayerId,
  onRoadPicked,
  onVertexPicked,
  onRobberHexPicked,
  onSettlementCityPicked,
}: Props) {
  const board = game.board;

  const pieceColorByPlayerId = useMemo(() => {
    const m = new Map<string, string>();
    for (const p of game.players) m.set(p.id, playerColorToHex(p.playerColor));
    return m;
  }, [game.players]);

  const occupiedVertexKeys = useMemo(() => {
    const s = new Set<string>();
    for (const pc of board.populationCenters) {
      if (pc.coordinates.length >= 3) {
        const [a, b, c] = pc.coordinates;
        s.add(vertexKey([a, b, c]));
      }
    }
    return s;
  }, [board.populationCenters]);

  const occupiedRoadKeys = useMemo(() => {
    const s = new Set<string>();
    for (const r of board.roads) {
      if (r.coordinates.length >= 2) {
        s.add(edgeKey(r.coordinates[0]!, r.coordinates[1]!));
      }
    }
    return s;
  }, [board.roads]);

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

  const allRoadCandidates = useMemo((): RoadCandidate[] => {
    const directions = [
      { q: 1, r: -1, s: 0 },
      { q: 1, r: 0, s: -1 },
      { q: 0, r: 1, s: -1 },
      { q: -1, r: 1, s: 0 },
      { q: -1, r: 0, s: 1 },
      { q: 0, r: -1, s: 1 },
    ] as const;

    const dedup = new Map<string, RoadCandidate>();

    for (const hex of board.hexes) {
      for (const d of directions) {
        const neighborCoord: HexCoordinate = {
          q: hex.coordinate.q + d.q,
          r: hex.coordinate.r + d.r,
          s: hex.coordinate.s + d.s,
        };

        const aKey = `${hex.coordinate.q},${hex.coordinate.r},${hex.coordinate.s}`;
        const bKey = `${neighborCoord.q},${neighborCoord.r},${neighborCoord.s}`;
        const key = aKey < bKey ? `${aKey}|${bKey}` : `${bKey}|${aKey}`;
        if (dedup.has(key)) continue;

        const pa = cubeToPosition(
          hexRadius,
          hex.coordinate.q,
          hex.coordinate.r,
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

        dedup.set(key, {
          key,
          x: m.x,
          z: m.z,
          angle,
          hexA: { ...hex.coordinate },
          hexB: { ...neighborCoord },
        });
      }
    }

    return Array.from(dedup.values());
  }, [board.hexes, hexRadius]);

  const hoverRoadCandidates = useMemo(() => {
    if (boardPickMode !== "build" || !showGhostRoads) return [];

    let list: RoadCandidate[];

    if (initialRoadVertexHexes && initialRoadVertexHexes.length >= 3) {
      list = allRoadCandidates.filter((c) =>
        edgeTouchesVertex(initialRoadVertexHexes, [c.hexA, c.hexB]),
      );
    } else if (buildableRoads !== undefined) {
      if (buildableRoads.length === 0) {
        list = [];
      } else {
        list = allRoadCandidates.filter((c) =>
          isEdgeInBuildableList(buildableRoads, c.hexA, c.hexB),
        );
      }
    } else {
      list = allRoadCandidates;
    }

    return list.filter(
      (c) => !occupiedRoadKeys.has(edgeKey(c.hexA, c.hexB)),
    );
  }, [
    allRoadCandidates,
    boardPickMode,
    showGhostRoads,
    buildableRoads,
    initialRoadVertexHexes,
    occupiedRoadKeys,
  ]);

  const hoverVertexCandidates = useMemo(() => {
    if (boardPickMode !== "build" || !showGhostSettlementVertices) return [];

    const directions = [
      { q: 1, r: -1, s: 0 },
      { q: 1, r: 0, s: -1 },
      { q: 0, r: 1, s: -1 },
      { q: -1, r: 1, s: 0 },
      { q: -1, r: 0, s: 1 },
      { q: 0, r: -1, s: 1 },
    ] as const;

    const dedup = new Map<string, VertexCandidate>();

    for (const hex of board.hexes) {
      const p0 = cubeToPosition(hexRadius, hex.coordinate.q, hex.coordinate.r);
      const h0 = { ...hex.coordinate };

      for (let i = 0; i < 6; i++) {
        const d1 = directions[i];
        const d2 = directions[(i + 5) % 6];

        const h1: HexCoordinate = {
          q: hex.coordinate.q + d1.q,
          r: hex.coordinate.r + d1.r,
          s: hex.coordinate.s + d1.s,
        };
        const h2: HexCoordinate = {
          q: hex.coordinate.q + d2.q,
          r: hex.coordinate.r + d2.r,
          s: hex.coordinate.s + d2.s,
        };

        const p1 = cubeToPosition(hexRadius, h1.q, h1.r);
        const p2 = cubeToPosition(hexRadius, h2.q, h2.r);

        const vx = (p0.x + p1.x + p2.x) / 3;
        const vz = (p0.z + p1.z + p2.z) / 3;

        const kx = Math.round(vx * 1000);
        const kz = Math.round(vz * 1000);
        const vKey = `${kx},${kz}`;

        if (!dedup.has(vKey)) {
          dedup.set(vKey, { key: vKey, x: vx, z: vz, h0, h1, h2 });
        }
      }
    }

    let list = Array.from(dedup.values());

    const bs = buildableSettlements;
    if (bs === undefined) {
      // Unrestricted (e.g. legacy); still hide occupied vertices.
    } else if (bs.length === 0) {
      list = [];
    } else {
      list = list.filter((v) =>
        isVertexInBuildableList(bs, v.h0, v.h1, v.h2),
      );
    }

    return list.filter(
      (v) => !occupiedVertexKeys.has(vertexKey([v.h0, v.h1, v.h2])),
    );
  }, [
    board.hexes,
    hexRadius,
    boardPickMode,
    showGhostSettlementVertices,
    buildableSettlements,
    occupiedVertexKeys,
  ]);

  const handleCityClick = (pc: PopulationCenter) => {
    if (pc.coordinates.length < 3) return;
    const [a, b, c] = pc.coordinates;
    onSettlementCityPicked?.(toVertexCoordDto(a, b, c));
  };

  const settlementUpgradeable =
    boardPickMode === "build" &&
    showGhostCityUpgrades &&
    !!myPlayerId;

  /**
   * 3D position (XZ centroid of the three surrounding hex centers) of the
   * setup-phase settlement that has been clicked but not yet finalized with
   * an adjacent road. Kept visible as a translucent marker so the player can
   * see where they committed while they pick the road.
   */
  const pendingInitialVertexPosition = useMemo<
    [number, number, number] | null
  >(() => {
    if (!initialRoadVertexHexes || initialRoadVertexHexes.length < 3) return null;
    const [h0, h1, h2] = initialRoadVertexHexes;
    const p0 = cubeToPosition(hexRadius, h0!.q, h0!.r);
    const p1 = cubeToPosition(hexRadius, h1!.q, h1!.r);
    const p2 = cubeToPosition(hexRadius, h2!.q, h2!.r);
    return [(p0.x + p1.x + p2.x) / 3, 0.22, (p0.z + p1.z + p2.z) / 3];
  }, [initialRoadVertexHexes, hexRadius]);

  const myPieceColor = myPlayerId
    ? pieceColorByPlayerId.get(myPlayerId)
    : undefined;

  /** Wide plane under the island so open water reads as ocean. */
  const oceanPlaneSize = useMemo(() => {
    let maxDist = hexRadius * 4;
    for (const h of board.hexes) {
      const p = cubeToPosition(hexRadius, h.coordinate.q, h.coordinate.r);
      maxDist = Math.max(maxDist, Math.hypot(p.x, p.z) + hexRadius * 2.5);
    }
    for (const port of board.ports) {
      const pa = cubeToPosition(
        hexRadius,
        port.inCoordinate.q,
        port.inCoordinate.r,
      );
      const pb = cubeToPosition(
        hexRadius,
        port.outCoordinate.q,
        port.outCoordinate.r,
      );
      maxDist = Math.max(
        maxDist,
        Math.hypot(pa.x, pa.z) + hexRadius,
        Math.hypot(pb.x, pb.z) + hexRadius,
      );
    }
    return maxDist * 2 + hexRadius * 6;
  }, [board.hexes, board.ports, hexRadius]);

  return (
    <div style={{ width: "100%", height: "100%" }}>
      <Canvas camera={{ position: [0, 5, 8], fov: 50 }}>
        <ambientLight intensity={0.6} />
        <directionalLight position={[5, 10, 5]} intensity={0.8} />
        <OrbitControls enableZoom enableRotate />

        {/* Ocean floor — sits just below hex bases */}
        <mesh
          receiveShadow
          rotation={[-Math.PI / 2, 0, 0]}
          position={[0, -0.11, 0]}
        >
          <planeGeometry args={[oceanPlaneSize, oceanPlaneSize]} />
          <meshStandardMaterial
            color="#0f5680"
            roughness={0.86}
            metalness={0.07}
          />
        </mesh>

        <group>
          {board.hexes.map((hex, idx) => (
            <HexTile
              key={`${hex.coordinate.q},${hex.coordinate.r},${hex.coordinate.s}-${idx}`}
              hex={hex}
              hexRadius={hexRadius}
              color={resourceColors[hex.resource] ?? "#cccccc"}
              hexNumber={hex.numberToken}
              robberPickMode={boardPickMode === "moveRobber"}
              onRobberHexPick={
                onRobberHexPicked
                  ? (h) => onRobberHexPicked(h.coordinate)
                  : undefined
              }
            />
          ))}

          {boardPickMode === "build" && showGhostRoads && (
            <group>
              {hoverRoadCandidates.map((c) => (
                <GhostRoadMesh
                  key={c.key}
                  position={[c.x, 0.2, c.z]}
                  angle={c.angle}
                  hexRadius={hexRadius}
                  onClick={() => onRoadPicked?.(toEdgeCoordDto(c.hexA, c.hexB))}
                />
              ))}
            </group>
          )}

          {boardPickMode === "build" && showGhostSettlementVertices && (
            <group>
              {hoverVertexCandidates.map((v) => (
                <GhostSettlementMesh
                  key={v.key}
                  position={[v.x, 0.22, v.z]}
                  onClick={() =>
                    onVertexPicked?.(toVertexCoordDto(v.h0, v.h1, v.h2))
                  }
                />
              ))}
            </group>
          )}

          {pendingInitialVertexPosition && (
            <GhostSettlementMesh
              position={pendingInitialVertexPosition}
              color={myPieceColor ?? "#ffffff"}
              alwaysVisible
            />
          )}

          {board.roads.map((road, idx) => (
            <RoadMesh
              key={`road-${idx}`}
              road={road}
              hexRadius={hexRadius}
              colorHex={pieceColorByPlayerId.get(road.playerOwnerId)}
            />
          ))}

          {board.populationCenters.map((pc, idx) =>
            pc.type === "city" ? (
              <CityMesh
                key={`pc-${idx}`}
                populationCenter={pc}
                hexRadius={hexRadius}
                colorHex={pieceColorByPlayerId.get(pc.playerOwnerId)}
              />
            ) : (
              <SettlementMesh
                key={`pc-${idx}`}
                populationCenter={pc}
                hexRadius={hexRadius}
                colorHex={pieceColorByPlayerId.get(pc.playerOwnerId)}
                upgradeable={
                  settlementUpgradeable &&
                  pc.playerOwnerId === myPlayerId
                }
                onUpgrade={handleCityClick}
              />
            ),
          )}

          {board.ports.map((port, idx) => (
            <PortMesh key={`port-${idx}`} port={port} hexRadius={hexRadius} />
          ))}
        </group>
      </Canvas>
    </div>
  );
}
