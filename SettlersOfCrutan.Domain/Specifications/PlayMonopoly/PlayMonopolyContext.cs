using SettlersOfCrutan.Domain.Games;

namespace SettlersOfCrutan.Domain.Specifications.PlayMonopoly;

public record PlayMonopolyContext(GamePhase GamePhase, PlayerId CurrentPlayerId, PlayerId ActingPlayerId, Player ActingPlayer);
