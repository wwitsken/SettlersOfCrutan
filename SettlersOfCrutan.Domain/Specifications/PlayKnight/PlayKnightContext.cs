using SettlersOfCrutan.Domain.Games;

namespace SettlersOfCrutan.Domain.Specifications.PlayKnight;

public record PlayKnightContext(GamePhase GamePhase, PlayerId CurrentPlayerId, PlayerId ActingPlayerId, Player ActingPlayer);
