using SettlersOfCrutan.Domain.Games;
using SettlersOfCrutan.Domain.Games.Boards;
using SettlersOfCrutan.Domain.Games.Resources;
using SettlersOfCrutan.Domain.Specifications.Maritime3to1Trade;

namespace SettlersOfCrutan.Domain.UnitTests.Specifications;

public class Maritime3to1TradeSpecTests
{
    private static Maritime3to1TradeContext MakeContext(
        GamePhase phase = GamePhase.TradeBuild,
        PlayerId? currentPlayerId = null,
        PlayerId? actingPlayerId = null,
        Player? actingPlayer = null,
        ResourceCardType discard = ResourceCardType.Brick,
        ResourceCardType request = ResourceCardType.Ore,
        ResourceHand? bank = null,
        Board? board = null)
    {
        var pid = actingPlayerId ?? new PlayerId { Value = "p1" };
        return new Maritime3to1TradeContext(
            phase,
            currentPlayerId ?? pid,
            pid,
            actingPlayer ?? Player.Create("p1"),
            discard,
            request,
            bank ?? ResourceHand.StandardBankResources(),
            board ?? new Board());
    }

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
        Assert.Equal("WrongTurn", result.Error.Code);
    }

    [Fact]
    public void PlayerMustHaveDiscardResources_HasThreeOrMore_Succeeds()
    {
        var player = Player.Create("p1");
        player.AddResource(ResourceCardType.Brick, 3);
        var result = new PlayerMustHaveDiscardResources().IsSatisfiedBy(
            MakeContext(actingPlayer: player, discard: ResourceCardType.Brick));
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void PlayerMustHaveDiscardResources_HasNone_Fails()
    {
        var player = Player.Create("p1");
        var result = new PlayerMustHaveDiscardResources().IsSatisfiedBy(
            MakeContext(actingPlayer: player, discard: ResourceCardType.Brick));
        Assert.True(result.IsFailure);
        Assert.Equal("InsufficientResources", result.Error.Code);
    }

    [Fact]
    public void BankMustHaveRequestedResource_BankHasResource_Succeeds()
    {
        var result = new BankMustHaveRequestedResource().IsSatisfiedBy(
            MakeContext(request: ResourceCardType.Ore, bank: ResourceHand.StandardBankResources()));
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void BankMustHaveRequestedResource_EmptyBank_Fails()
    {
        var result = new BankMustHaveRequestedResource().IsSatisfiedBy(
            MakeContext(request: ResourceCardType.Ore, bank: new ResourceHand()));
        Assert.True(result.IsFailure);
        Assert.Equal("InsufficientResources", result.Error.Code);
    }

    [Fact]
    public void PlayerMustHave3to1Port_NoSettlementOnPort_Fails()
    {
        var pid = new PlayerId { Value = "p1" };
        var result = new PlayerMustHave3to1Port().IsSatisfiedBy(
            MakeContext(board: new Board(), actingPlayerId: pid, currentPlayerId: pid));
        Assert.True(result.IsFailure);
        Assert.Equal("MaritimeTrade", result.Error.Code);
    }
}
