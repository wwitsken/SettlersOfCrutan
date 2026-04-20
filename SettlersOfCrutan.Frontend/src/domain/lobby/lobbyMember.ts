export type LobbyMember = {
  id: string;
  displayName?: string;
  preferredColor?: string;
  isMe: boolean;
  isHost: boolean;
  isReady: boolean;
};
