using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.Games.DomainEvents;
using SettlersOfCrutan.Domain.Specifications;
using JoinSpecs = SettlersOfCrutan.Domain.Specifications.JoinPlayer;
using LeaveSpecs = SettlersOfCrutan.Domain.Specifications.LeavePlayer;
using SetNameSpecs = SettlersOfCrutan.Domain.Specifications.PlayerSetName;
using SetColorSpecs = SettlersOfCrutan.Domain.Specifications.PlayerSetColor;
using SetReadySpecs = SettlersOfCrutan.Domain.Specifications.PlayerSetReady;

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

    private static readonly ISpecification<JoinSpecs.JoinPlayerContext>[] JoinPlayerSpecifications =
    [
        new JoinSpecs.PlayerWithUserIdMustExist()
    ];

    public Result<PlayerId> JoinPlayer(string userId, DateTimeOffset when)
    {
        var p = _players.SingleOrDefault(x => x.UserId == userId);
        var context = new JoinSpecs.JoinPlayerContext(Id, p);

        foreach (var spec in JoinPlayerSpecifications)
        {
            var result = spec.IsSatisfiedBy(context);
            if (result.IsFailure) return Result.Failure<PlayerId>(result.Error);
        }

        p!.JoinedAt ??= when;
        AddDomainEvent(new PlayerJoinedDomainEvent(Id, p.Id, when));
        return Result.Success(p.Id);
    }

    private static readonly ISpecification<LeaveSpecs.LeavePlayerContext>[] LeavePlayerSpecifications =
    [
        new LeaveSpecs.PlayerMustExist()
    ];

    public Result<PlayerId> LeavePlayer(PlayerId playerId, DateTimeOffset when)
    {
        var p = _players.SingleOrDefault(x => x.Id == playerId);
        var context = new LeaveSpecs.LeavePlayerContext(Id, playerId, p);

        foreach (var spec in LeavePlayerSpecifications)
        {
            var result = spec.IsSatisfiedBy(context);
            if (result.IsFailure) return Result.Failure<PlayerId>(result.Error);
        }

        p!.JoinedAt = null;
        AddDomainEvent(new PlayerLeftDomainEvent(Id, playerId, when));
        return Result.Success(playerId);
    }

    private static readonly ISpecification<SetNameSpecs.PlayerSetNameContext>[] PlayerSetNameSpecifications =
    [
        new SetNameSpecs.PlayerMustExist()
    ];

    public Result<PlayerId> PlayerSetName(PlayerId playerId, string name)
    {
        var p = _players.SingleOrDefault(x => x.Id == playerId);
        var context = new SetNameSpecs.PlayerSetNameContext(Id, playerId, p);

        foreach (var spec in PlayerSetNameSpecifications)
        {
            var result = spec.IsSatisfiedBy(context);
            if (result.IsFailure) return Result.Failure<PlayerId>(result.Error);
        }

        p!.SetName(name);
        AddDomainEvent(new PlayerSetNameDomainEvent(Id, playerId, name));
        return Result.Success(playerId);
    }

    private static readonly ISpecification<SetColorSpecs.PlayerSetColorContext>[] PlayerSetColorSpecifications =
    [
        new SetColorSpecs.PlayerMustExist(),
        new SetColorSpecs.ColorMustNotBeNone(),
        new SetColorSpecs.ColorMustNotBeTaken()
    ];

    public Result<PlayerId> PlayerSetColor(PlayerId playerId, PlayerColor color)
    {
        var p = _players.SingleOrDefault(x => x.Id == playerId);
        var context = new SetColorSpecs.PlayerSetColorContext(Id, playerId, p, color, Players);

        foreach (var spec in PlayerSetColorSpecifications)
        {
            var result = spec.IsSatisfiedBy(context);
            if (result.IsFailure) return Result.Failure<PlayerId>(result.Error);
        }

        p!.SetColor(color);
        AddDomainEvent(new PlayerSetColorDomainEvent(Id, playerId, color));
        return Result.Success(playerId);
    }

    private static readonly ISpecification<SetReadySpecs.PlayerSetReadyContext>[] PlayerSetReadySpecifications =
    [
        new SetReadySpecs.PlayerMustExist(),
        new SetReadySpecs.PlayerMustHaveColorBeforeReady(),
        new SetReadySpecs.PlayerMustHaveNameBeforeReady()
    ];

    public Result<PlayerId> PlayerSetReady(PlayerId playerId, bool ready)
    {
        var p = _players.SingleOrDefault(x => x.Id == playerId);
        var context = new SetReadySpecs.PlayerSetReadyContext(Id, playerId, p, ready);

        foreach (var spec in PlayerSetReadySpecifications)
        {
            var result = spec.IsSatisfiedBy(context);
            if (result.IsFailure) return Result.Failure<PlayerId>(result.Error);
        }

        p!.SetReady(ready);
        AddDomainEvent(new PlayerSetReadyDomainEvent(Id, playerId, ready));
        return Result.Success(playerId);
    }
}
