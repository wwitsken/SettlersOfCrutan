import { Canvas } from "@react-three/fiber";
import { OrbitControls } from "@react-three/drei";
import type { Game } from "../../domain/game/game";
import { HexTile } from "./HexTile";
import { RoadMesh } from "./RoadMesh";
import { PopulationCenterMesh } from "./PopulationCenterMesh";
import { PortMesh } from "./PortMesh";

type Props = {
  game: Game;
  hexRadius?: number;
};

export function CatanBoardScene({ game, hexRadius = 1 }: Props) {
  const board = game.board;

  const resourceColors: Record<string, string> = {
    wood: "#2e8b57",
    brick: "#b7410e",
    sheep: "#9acd32",
    wheat: "#f0e68c",
    ore: "#808080",
    desert: "#d2b48c",
    water: "#1e90ff",
  };

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
            />
          ))}

          {board.roads.map((road, idx) => (
            <RoadMesh key={`road-${idx}`} road={road} hexRadius={hexRadius} />
          ))}

          {board.populationCenters.map((pc, idx) => (
            <PopulationCenterMesh
              key={`pc-${idx}`}
              populationCenter={pc}
              hexRadius={hexRadius}
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
