using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.Games.Boards;
using SettlersOfCrutan.Domain.Games.DomainEvents;
using SettlersOfCrutan.Domain.Generation;

namespace SettlersOfCrutan.Domain.Games;

public record GameId : BaseId<Guid>;
public enum PlayerDirection { Clockwise, CounterClockwise }
public partial class Game : AggregateRoot<GameId>
{
    public override GameId Id { get; init; } = new() { Value = Guid.NewGuid() };
    public required GameType GameType { get; set; }
    public required string Name { get; set; }
    public required Board Board { get; set; }
    public required GamePhase GamePhase { get; set; }
    public required ResourceBag Bank { get; set; } = ResourceBag.StandardBank();

    private List<Player> _players = [];
    public List<Player> Players
    {
        get => [.. _players.OrderBy(p => p.Id.Value)];
        set => _players = value;
    }

    private List<DiscardHalfRequirement> _discardHalfRequirements = [];
    public List<DiscardHalfRequirement> DiscardHalfRequirements
    {
        get => [.. _discardHalfRequirements];
        set => _discardHalfRequirements = value;
    }

    public PlayerId CurrentPlayerId() => Players[PlayerIndex].Id;
    public DateTimeOffset? TurnExpiresAt { get; set; }
    public PlayerDirection PlayerDirection { get; set; } = PlayerDirection.Clockwise;
    public int Round { get; set; } = 1;
    public int PlayerIndex { get; set; } = 0;
    public TradeOffer? CurrentTradeOffer { get; set; } = null;

    public bool AllPlayersJoined() => Players.All(p => p.JoinedAt is not null);
    public Result<PlayerId> JoinPlayer(PlayerId playerId, DateTimeOffset when)
    {
        var p = _players.SingleOrDefault(x => x.Id == playerId);
        if (p is null) return Result.Failure<PlayerId>(new("PlayerNotFound", $"Player with id {playerId} not found in game {Id}"));
        p.JoinedAt ??= when;
        AddDomainEvent(new PlayerJoinedDomainEvent(Id, playerId, when));
        return Result.Success(playerId);
    }

    public static Result<Game> CreateGame(string gameName, string[] userIds, IBoardGenerator boardGenerator)
    {
        Game game = new()
        {
            GameType = GameType.BaseGame,
            GamePhase = GamePhase.Setup,
            Name = gameName,
            Bank = ResourceBag.StandardBank(),
            Players = [.. userIds.Select(id => Player.Create(id, ResourceBag.StandardPlayerStartingBag()))],
            Board = boardGenerator.Generate(StandardBoardConfigurations.DefaultBaseGame, Environment.TickCount)
        };

        game.AddDomainEvent(new GameCreatedDomainEvent(game.Id, [.. game.Players.Select(p => p.Id.Value)]));

        return Result.Success(game);
    }
}
