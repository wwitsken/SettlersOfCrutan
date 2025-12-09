using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.Games.Boards;
using SettlersOfCrutan.Domain.Games.DomainEvents;
using SettlersOfCrutan.Domain.Games.Generation;
using SettlersOfCrutan.Domain.Games.Resources;
using System.Text.Json.Serialization;

namespace SettlersOfCrutan.Domain.Games;

public record GameId : BaseId<Guid>;
public enum PlayerDirection { Clockwise, CounterClockwise }
public partial class Game : AggregateRoot<GameId>
{
    public override GameId Id { get; init; } = new() { Value = Guid.NewGuid() };
    public GameType GameType { get; set; }
    public string GameName { get; set; }
    public Board Board { get; set; }

    // Separate bank stocks
    public ResourceHand BankResourceHand { get; private set; } = ResourceHand.StandardBankResources();
    public DevCardHand BankDevCardHand { get; private set; } = DevCardHand.StandardBankDeck();

    public bool AllPlayersJoined() => Players.All(p => p.JoinedAt is not null);
    public bool AllPlayersReady() => Players.All(p => p.Ready);
    public PlayerId CurrentPlayerId() => Players[PlayerIndex].Id;
    public DateTimeOffset? TurnExpiresAt { get; set; }
    public PlayerDirection PlayerDirection { get; set; } = PlayerDirection.Clockwise;
    public GamePhase GamePhase { get; set; }
    public int Round { get; set; } = 1;
    public int PlayerIndex { get; set; } = 0;
    public TradeOffer? CurrentTradeOffer { get; set; } = null;

    private readonly List<Player> _players = [];
    public IReadOnlyList<Player> Players => [.. _players];

    private readonly List<DiscardHalfRequirement> _discardHalfRequirements = [];
    public IReadOnlyList<DiscardHalfRequirement> DiscardHalfRequirements => [.. _discardHalfRequirements];

    [JsonConstructor]
    private Game(
        GameType gameType,
        string gameName,
        Board board,
        GamePhase gamePhase,
        ResourceHand bankResourceHand,
        DevCardHand bankDevCardHand,
        IReadOnlyList<Player> players,
        IReadOnlyList<DiscardHalfRequirement> discardHalfRequirements,
        PlayerDirection playerDirection = PlayerDirection.Clockwise,
        int round = 1,
        int playerIndex = 0,
        TradeOffer? currentTradeOffer = null,
        DateTimeOffset? turnExpiresAt = null
    )
    {
        GameType = gameType;
        GameName = gameName;
        Board = board;
        GamePhase = gamePhase;
        BankResourceHand = bankResourceHand;
        BankDevCardHand = bankDevCardHand;
        _players = [.. players];
        _discardHalfRequirements = [.. discardHalfRequirements];
        PlayerDirection = playerDirection;
        Round = round;
        PlayerIndex = playerIndex;
        CurrentTradeOffer = currentTradeOffer;
        TurnExpiresAt = turnExpiresAt;
    }

    public static Result<Game> CreateGame(string gameName, string[] userIds, IBoardGenerator boardGenerator)
    {
        Game game = new(
            GameType.BaseGame,
            gameName,
            boardGenerator.Generate(StandardBoardConfigurations.DefaultBaseGame, Environment.TickCount),
            GamePhase.Setup,
            ResourceHand.StandardBankResources(),
            DevCardHand.StandardBankDeck(),
            [.. userIds.Select(Player.Create)],
            [],
            PlayerDirection.Clockwise,
            1,
            0
        );

        game.AddDomainEvent(new GameCreatedDomainEvent(game.Id, [.. game.Players.Select(p => p.Id.Value)]));

        return Result.Success(game);
    }
}
