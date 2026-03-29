using SettlersOfCrutan.Domain.Games;
using SettlersOfCrutan.Domain.Specifications.EndTurn;

namespace SettlersOfCrutan.Domain.UnitTests.Specifications;

public class EndTurnSpecTests
{
    private static EndTurnContext MakeContext(
        GamePhase phase = GamePhase.TradeBuild,
        PlayerId? currentPlayerId = null,
        PlayerId? actingPlayerId = null)
    {
        var pid = actingPlayerId ?? new PlayerId { Value = "p1" };
        return new EndTurnContext(phase, currentPlayerId ?? pid, pid);
    }

    [Fact]
    public void GameMustBeInSetupOrTradeBuild_Setup_Succeeds()
    {
        var result = new GameMustBeInSetupOrTradeBuild().IsSatisfiedBy(MakeContext(phase: GamePhase.Setup));
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void GameMustBeInSetupOrTradeBuild_TradeBuild_Succeeds()
    {
        var result = new GameMustBeInSetupOrTradeBuild().IsSatisfiedBy(MakeContext(phase: GamePhase.TradeBuild));
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void GameMustBeInSetupOrTradeBuild_RollDice_Fails()
    {
        var result = new GameMustBeInSetupOrTradeBuild().IsSatisfiedBy(MakeContext(phase: GamePhase.RollDice));
        Assert.True(result.IsFailure);
        Assert.Equal("WrongGamePhase", result.Error.Code);
    }

    [Fact]
    public void MustBeCurrentPlayerTurn_SamePlayer_Succeeds()
    {
        var pid = new PlayerId { Value = "p1" };
        var result = new MustBeCurrentPlayerTurn().IsSatisfiedBy(MakeContext(currentPlayerId: pid, actingPlayerId: pid));
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void MustBeCurrentPlayerTurn_DifferentPlayer_Fails()
    {
        var result = new MustBeCurrentPlayerTurn().IsSatisfiedBy(
            MakeContext(currentPlayerId: new PlayerId { Value = "p1" }, actingPlayerId: new PlayerId { Value = "p2" }));
        Assert.True(result.IsFailure);
    }
}
