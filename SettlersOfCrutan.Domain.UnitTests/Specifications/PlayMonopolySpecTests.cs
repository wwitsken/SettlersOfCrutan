using SettlersOfCrutan.Domain.Games;
using SettlersOfCrutan.Domain.Games.Resources;
using SettlersOfCrutan.Domain.Specifications.PlayMonopoly;

namespace SettlersOfCrutan.Domain.UnitTests.Specifications;

public class PlayMonopolySpecTests
{
    private static readonly PlayerId P1 = new() { Value = "p1" };

    private static PlayMonopolyContext MakeContext(
        GamePhase phase = GamePhase.TradeBuild,
        PlayerId? currentPlayerId = null,
        PlayerId? actingPlayerId = null,
        Player? actingPlayer = null) =>
        new(
            phase,
            currentPlayerId ?? P1,
            actingPlayerId ?? P1,
            actingPlayer ?? Player.Create("p1"));

    [Fact]
    public void GameMustBeInTradeBuildPhase_TradeBuild_Succeeds()
    {
        var result = new GameMustBeInTradeBuildPhase().IsSatisfiedBy(MakeContext(phase: GamePhase.TradeBuild));
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void GameMustBeInTradeBuildPhase_Setup_Fails()
    {
        var result = new GameMustBeInTradeBuildPhase().IsSatisfiedBy(MakeContext(phase: GamePhase.Setup));
        Assert.True(result.IsFailure);
        Assert.Equal("WrongGamePhase", result.Error.Code);
    }

    [Fact]
    public void MustBeCurrentPlayerTurn_SamePlayer_Succeeds()
    {
        var result = new MustBeCurrentPlayerTurn().IsSatisfiedBy(MakeContext(currentPlayerId: P1, actingPlayerId: P1));
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void MustBeCurrentPlayerTurn_DifferentPlayer_Fails()
    {
        var result = new MustBeCurrentPlayerTurn().IsSatisfiedBy(
            MakeContext(currentPlayerId: P1, actingPlayerId: new PlayerId { Value = "p2" }));
        Assert.True(result.IsFailure);
        Assert.Equal("WrongTurn", result.Error.Code);
    }

    [Fact]
    public void PlayerMustHaveMonopolyCard_HasCard_Succeeds()
    {
        var player = Player.Create("p1");
        player.AddDevCard(DevelopmentCardType.Monopoly);
        var result = new PlayerMustHaveMonopolyCard().IsSatisfiedBy(MakeContext(actingPlayer: player));
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void PlayerMustHaveMonopolyCard_NoCard_Fails()
    {
        var result = new PlayerMustHaveMonopolyCard().IsSatisfiedBy(MakeContext(actingPlayer: Player.Create("p1")));
        Assert.True(result.IsFailure);
        Assert.Equal("DevCard", result.Error.Code);
    }
}
