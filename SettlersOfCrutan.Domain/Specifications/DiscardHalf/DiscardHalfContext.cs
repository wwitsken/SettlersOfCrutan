using SettlersOfCrutan.Domain.Games;
using SettlersOfCrutan.Domain.Games.Resources;

namespace SettlersOfCrutan.Domain.Specifications.DiscardHalf;

public record DiscardHalfContext(
    GamePhase GamePhase,
    DiscardHalfRequirement? Requirement,
    List<ResourceCardAmount>? Discards,
    int DiscardTotal,
    Player? Player);
