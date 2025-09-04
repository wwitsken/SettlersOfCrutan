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
    public required string Name { get; set; } = "Catan Game";
    public required Board Board { get; set; }
    public required GamePhase GamePhase { get; set; }

    private List<Player> _players = [];
    public List<Player> Players
    {
        get => [.. _players.OrderBy(p => p.Id.Value)];
        set => _players = value;
    }
    public List<DiscardHalfRequirement> DiscardHalfRequirements { get; set; } = [];
    public List<PlayerId> PlayersNeedingToDiscardHalf => [.. DiscardHalfRequirements.Select(r => r.PlayerId)];
    public PlayerId CurrentPlayerId() => Players[PlayerIndex].Id;
    public DateTimeOffset? TurnExpiresAt { get; set; }
    public PlayerDirection PlayerDirection { get; set; } = PlayerDirection.Clockwise;
    public int Round { get; set; } = 1;
    public int PlayerIndex { get; set; } = 0;

    /*
    public Result<Nothing> BuildRoad(Game game, PlayerId playerId, Edge edge)
    {
        // Ensure the player has enough resources (1 Brick, 1 Lumber) & 1 Road piece
        // Try putting the road down on the board
        // Deduct resources and road piece from player if successful
        throw new NotImplementedException();
    }
    public Result<Nothing> BuildSettlement(Game game, PlayerId playerId, Vertex vertex)
    {
        throw new NotImplementedException();
    }
    public Result<Nothing> BuildCity(Game game, PlayerId playerId, Vertex vertex)
    {
        throw new NotImplementedException();
    }
    public Result<Nothing> MakeMaritimeTrade()
    {
        throw new NotImplementedException();
    }
    */

    public Result<Nothing> EndTurnAndStartNextTurn(IDateTimeProvider clock, TimeSpan? turnDuration = null)
    {
        throw new NotImplementedException();

        //NextPlayer();
        //CurrentTurnDetails = new TurnDetails()
        //{ GamePhase = GamePhase == GamePhase.Normal ? GamePhase.RollDice : GamePhase.Setup, Status = TurnStatus.Active };
        //CurrentTurnDetails.Start(clock, turnDuration);
        ////BumpVersion(); // move all bumpversions to Applicaiton layer
        //return Result.Success();
    }

    public static Result<Game> CreateGame(string gameName, string[] playerIds, IBoardGenerator boardGenerator)
    {
        Game game = new()
        {
            GamePhase = GamePhase.Setup,
            Name = gameName,
            Players = [.. playerIds.Select((id, ix) => Player.Create(ix, id))],
            Board = boardGenerator.Generate(StandardBoardConfigurations.DefaultBaseGame, Environment.TickCount)
        };

        game.AddDomainEvent(new GameCreatedDomainEvent(game.Id, [.. game.Players.Select(p => p.UserId)]));

        return Result.Success(game);
    }
}
