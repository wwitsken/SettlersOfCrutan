using SettlersOfCrutan.Domain.Games;

namespace SettlersOfCrutan.Domain.Specifications.PlayerSetName;

public record PlayerSetNameContext(GameId GameId, PlayerId PlayerId, Player? Player);
