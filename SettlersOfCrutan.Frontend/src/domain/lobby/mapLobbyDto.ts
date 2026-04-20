import type { Lobby } from "./lobby";
import type { components } from "../../api/types";

type LobbyDto = components["schemas"]["LobbyDto"];
type User = components["schemas"]["UserProfileDto"];

export function lobbyDtoToDomain(data: LobbyDto, users: User[]): Lobby | null {
  const id = data.lobbyId;
  if (!id) return null;
  return {
    lobbyId: id,
    lobbyMembers: (data.lobbyMembers ?? []).map((m) => ({
      id: m.id!,
      displayName: users.find((u) => u.userId === m.userId!)?.displayName,
      preferredColor: users.find((u) => u.userId === m.userId!)?.preferredColor,
      isMe: !!m.isMe,
      isHost: !!m.isHost,
      isReady: !!m.isReady,
    })),
  };
}
