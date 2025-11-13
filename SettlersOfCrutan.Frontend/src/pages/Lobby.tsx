import { useLoaderData } from "react-router";
import type { components } from "../api/types";

export type LobbyData = {
  status?: number | null;
  data?: components["schemas"]["LobbyDto"];
};

function Lobby() {
  const { status: errorCode, data } = useLoaderData<LobbyData>();
  return (
    <div>
      <h1>Catan Lobby</h1>
      <h2>{data?.lobbyId}</h2>
      <h2>{errorCode}</h2>
      <ul className="flex flex-col space-y-4">
        {data?.lobbyPlayers?.map((p) => (
          <li>{p.gameName || "no name"}</li>
        ))}
      </ul>
    </div>
  );
}

export default Lobby;
