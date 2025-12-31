import type { LobbyMember } from "./lobbyMembers";

export type Lobby = {
  lobbyId: string;
  lobbyMembers: LobbyMember[];
};
