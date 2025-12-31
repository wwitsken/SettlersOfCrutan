using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.DomainErrors;
using SettlersOfCrutan.Domain.Games.DomainEvents;

namespace SettlersOfCrutan.Domain.Games;
public partial class Game
{
    public Result<PlayerId> StartGame(IDateTimeProvider clock, TimeSpan? turnDuration = null)
    {
        GamePhase = GamePhase.Setup;
        PlayerIndex = 0;
        Round = 1;
        TurnExpiresAt = turnDuration.HasValue ? clock.UtcNow.Add(turnDuration.Value) : null;
        AddDomainEvent(new GameStartedDomainEvent(Id, CurrentPlayerId()));
        return Result.Success(CurrentPlayerId());
    }

    public Result<PlayerId> JoinPlayer(PlayerId playerId, DateTimeOffset when)
    {
        var p = _players.SingleOrDefault(x => x.Id == playerId);
        if (p is null) return Result.Failure<PlayerId>(DomainError.PlayerNotFound(Id, playerId));
        p.JoinedAt ??= when;
        AddDomainEvent(new PlayerJoinedDomainEvent(Id, playerId, when));
        return Result.Success(playerId);
    }

    public Result<PlayerId> LeavePlayer(PlayerId playerId, DateTimeOffset when)
    {
        var p = _players.SingleOrDefault(x => x.Id == playerId);
        if (p is null) return Result.Failure<PlayerId>(DomainError.PlayerNotFound(Id, playerId));
        p.JoinedAt = null;
        AddDomainEvent(new PlayerLeftDomainEvent(Id, playerId, when));
        return Result.Success(playerId);
    }

    public Result<PlayerId> PlayerSetName(PlayerId playerId, string name)
    {
        var p = _players.SingleOrDefault(x => x.Id == playerId);
        if (p is null) return Result.Failure<PlayerId>(DomainError.PlayerNotFound(Id, playerId));
        p.SetName(name);
        AddDomainEvent(new PlayerSetNameDomainEvent(Id, playerId, name));
        return Result.Success(playerId);
    }

    public Result<PlayerId> PlayerSetColor(PlayerId playerId, PlayerColor color)
    {
        var p = _players.SingleOrDefault(x => x.Id == playerId);
        if (p is null) return Result.Failure<PlayerId>(DomainError.PlayerNotFound(Id, playerId));
        if (color == PlayerColor.None)
            return Result.Failure<PlayerId>(DomainError.InvalidColor);
        if (_players.Any(x => x.Color == color && x.Id != playerId && color != PlayerColor.None))
            return Result.Failure<PlayerId>(DomainError.ColorTaken);
        p.SetColor(color);
        AddDomainEvent(new PlayerSetColorDomainEvent(Id, playerId, color));
        return Result.Success(playerId);
    }

    public Result<PlayerId> PlayerSetReady(PlayerId playerId, bool ready)
    {
        var p = _players.SingleOrDefault(x => x.Id == playerId);
        if (p is null) return Result.Failure<PlayerId>(DomainError.PlayerNotFound(Id, playerId));
        if (p.Color == PlayerColor.None && ready)
            return Result.Failure<PlayerId>(DomainError.ColorMustSetBeforeReady);
        if (string.IsNullOrEmpty(p.DisplayName) && ready)
            return Result.Failure<PlayerId>(DomainError.NameMustSetBeforeReady);
        p.SetReady(ready);
        AddDomainEvent(new PlayerSetReadyDomainEvent(Id, playerId, ready));
        return Result.Success(playerId);
    }
}