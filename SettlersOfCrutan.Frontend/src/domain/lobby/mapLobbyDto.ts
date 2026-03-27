import type { Lobby } from "./lobby";
import type { components } from "../../api/types";

type LobbyDto = components["schemas"]["LobbyDto"];

export function lobbyDtoToDomain(data: LobbyDto): Lobby | null {
  const id = data.lobbyId;
  if (!id) return null;
  return {
    lobbyId: id,
    lobbyMembers: (data.lobbyMembers ?? []).map((m) => ({
      id: m.id!,
      displayName: m.displayName ?? undefined,
      isMe: !!m.isMe,
      isHost: !!m.isHost,
      isReady: !!m.isReady,
    })),
  };
}
