export type LobbyMember = {
  id: string;
  displayName?: string;
  isMe: boolean;
  isHost: boolean;
  isReady: boolean;
};
