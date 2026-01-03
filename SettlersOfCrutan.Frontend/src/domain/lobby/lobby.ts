import type { LobbyMember } from "./lobbyMember";

export type Lobby = {
  lobbyId: string;
  lobbyMembers: LobbyMember[];
};
