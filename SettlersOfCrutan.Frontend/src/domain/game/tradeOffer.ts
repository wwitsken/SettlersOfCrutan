export type TradeOffer = {
  playerProposerId: string;
  playerAcceptorId: string | undefined;
  requestedResources: Record<string, number>;
  offeredResources: Record<string, number>;
  isAccepted: boolean;
};
