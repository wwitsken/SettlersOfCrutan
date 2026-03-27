import { game } from "../domain/game/gameExample";
import { CatanBoardScene } from "../components/board/CatanBoardScene";

function GamePage() {
  return (
    <div style={{ width: "100%", height: "calc(100vh - 80px)" }}>
      <CatanBoardScene game={game} hexRadius={1} />
    </div>
  );
}

export default GamePage;
