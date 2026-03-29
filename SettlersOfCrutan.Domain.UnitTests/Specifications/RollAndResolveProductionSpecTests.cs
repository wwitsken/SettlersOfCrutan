using SettlersOfCrutan.Domain.Games;
using SettlersOfCrutan.Domain.Specifications.RollAndResolveProduction;

namespace SettlersOfCrutan.Domain.UnitTests.Specifications;

public class RollAndResolveProductionSpecTests
{
    private static RollAndResolveProductionContext MakeContext(
        GamePhase phase = GamePhase.RollDice,
        PlayerId? currentPlayerId = null,
        PlayerId? actingPlayerId = null)
    {
        var pid = actingPlayerId ?? new PlayerId { Value = "p1" };
        return new RollAndResolveProductionContext(phase, currentPlayerId ?? pid, pid);
    }

    [Fact]
    public void GameMustBeInRollDicePhase_RollDice_Succeeds()
    {
        var result = new GameMustBeInRollDicePhase().IsSatisfiedBy(MakeContext(phase: GamePhase.RollDice));
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void GameMustBeInRollDicePhase_TradeBuild_Fails()
    {
        var result = new GameMustBeInRollDicePhase().IsSatisfiedBy(MakeContext(phase: GamePhase.TradeBuild));
        Assert.True(result.IsFailure);
        Assert.Equal("Roll", result.Error.Code);
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
