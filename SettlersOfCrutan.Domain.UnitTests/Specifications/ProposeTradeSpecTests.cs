using SettlersOfCrutan.Domain.Games;
using SettlersOfCrutan.Domain.Games.Resources;
using SettlersOfCrutan.Domain.Specifications.ProposeTrade;

namespace SettlersOfCrutan.Domain.UnitTests.Specifications;

public class ProposeTradeSpecTests
{
    private static ProposeTradeContext MakeContext(
        GamePhase phase = GamePhase.TradeBuild,
        PlayerId? currentPlayerId = null,
        PlayerId? actingPlayerId = null,
        TradeOffer? currentTradeOffer = null,
        Player? proposer = null,
        List<ResourceCardAmount>? offered = null)
    {
        var pid = actingPlayerId ?? new PlayerId { Value = "p1" };
        return new ProposeTradeContext(
            phase,
            currentPlayerId ?? pid,
            pid,
            currentTradeOffer,
            proposer,
            offered ?? [new ResourceCardAmount(ResourceCardType.Brick, 1)]);
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
    public void NoActiveTradeOffer_NullOffer_Succeeds()
    {
        var result = new NoActiveTradeOffer().IsSatisfiedBy(MakeContext(currentTradeOffer: null));
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void NoActiveTradeOffer_ActiveOffer_Fails()
    {
        var p = Player.Create(TestIds.User(1));
        p.AddResource(ResourceCardType.Brick, 1);
        var create = TradeOffer.Create(
            p,
            [new ResourceCardAmount(ResourceCardType.Ore, 1)],
            [new ResourceCardAmount(ResourceCardType.Brick, 1)]);
        Assert.True(create.IsSuccess);
        var offer = create.Value;

        var result = new NoActiveTradeOffer().IsSatisfiedBy(MakeContext(currentTradeOffer: offer));
        Assert.True(result.IsFailure);
        Assert.Equal("TradeOffer", result.Error.Code);
    }

    [Fact]
    public void ProposerMustExist_HasProposer_Succeeds()
    {
        var result = new ProposerMustExist().IsSatisfiedBy(MakeContext(proposer: Player.Create(TestIds.User(1))));
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void ProposerMustExist_NullProposer_Fails()
    {
        var result = new ProposerMustExist().IsSatisfiedBy(MakeContext(proposer: null));
        Assert.True(result.IsFailure);
        Assert.Equal("NotFound", result.Error.Code);
    }

    [Fact]
    public void ProposerMustHaveResources_PlayerHasOffered_Succeeds()
    {
        var proposer = Player.Create(TestIds.User(1));
        proposer.AddResource(ResourceCardType.Brick, 1);
        var offered = new List<ResourceCardAmount> { new(ResourceCardType.Brick, 1) };
        var result = new ProposerMustHaveResources().IsSatisfiedBy(MakeContext(proposer: proposer, offered: offered));
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void ProposerMustHaveResources_EmptyPlayer_Fails()
    {
        var proposer = Player.Create(TestIds.User(1));
        var offered = new List<ResourceCardAmount> { new(ResourceCardType.Brick, 1) };
        var result = new ProposerMustHaveResources().IsSatisfiedBy(MakeContext(proposer: proposer, offered: offered));
        Assert.True(result.IsFailure);
        Assert.Equal("InsufficientResources", result.Error.Code);
    }
}
