using SettlersOfCrutan.Domain.Games;

namespace SettlersOfCrutan.Domain.Specifications.PlayerSetColor;

public record PlayerSetColorContext(
    GameId GameId,
    PlayerId PlayerId,
    Player? Player,
    PlayerColor Color,
    IReadOnlyList<Player> AllPlayers);
