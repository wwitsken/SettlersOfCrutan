using SettlersOfCrutan.Domain.Games;

namespace SettlersOfCrutan.Domain.Specifications.JoinPlayer;

public record JoinPlayerContext(GameId GameId, Player? Player);
