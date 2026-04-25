using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.Games.DomainEvents;

namespace SettlersOfCrutan.Domain.Games;

public partial class Game
{
    /// <summary>
    /// Transitions the game to <see cref="GamePhase.GameEnd"/>, records the winner, and raises
    /// <see cref="PlayerWonDomainEvent"/>. No-op if the game is already ended.
    /// </summary>
    public Result<Nothing> DeclareWinner(PlayerId winnerId)
    {
        if (GamePhase == GamePhase.GameEnd) return Result.Success();
        if (_players.All(p => p.Id != winnerId))
            return Result.Failure(DomainError.PlayerNotFound(Id, winnerId));

        WinnerPlayerId = winnerId;
        GamePhase = GamePhase.GameEnd;
        AddDomainEvent(new PlayerWonDomainEvent(Id, winnerId));
        return Result.Success();
    }
}
