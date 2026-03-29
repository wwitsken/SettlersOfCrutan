using SettlersOfCrutan.Domain.Games;

namespace SettlersOfCrutan.Domain.Specifications.LeavePlayer;

public record LeavePlayerContext(GameId GameId, PlayerId PlayerId, Player? Player);
