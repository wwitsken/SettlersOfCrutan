import { api } from "./client";

export async function getGame(id: string) {
  const { data, error } = await api.GET("/api/lobby/{lobbyId}", {
    params: { path: { lobbyId: id } },
  });

  if (error) throw error; // typed to your error responses
  data?.lobbyPlayers?.find((v) => v.isHost);
  return data; // typed to 200 response body
}
