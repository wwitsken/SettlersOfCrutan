using SettlersOfCrutan.Domain.Games;

namespace SettlersOfCrutan.Domain.Specifications.PlayerSetReady;

public record PlayerSetReadyContext(GameId GameId, PlayerId PlayerId, Player? Player, bool Ready);
