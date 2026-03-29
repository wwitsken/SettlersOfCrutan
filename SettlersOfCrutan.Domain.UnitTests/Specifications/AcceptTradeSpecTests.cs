using SettlersOfCrutan.Domain.Games;
using SettlersOfCrutan.Domain.Games.Resources;
using SettlersOfCrutan.Domain.Specifications.AcceptTrade;

namespace SettlersOfCrutan.Domain.UnitTests.Specifications;

public class AcceptTradeSpecTests
{
    private static AcceptTradeContext MakeContext(
        GamePhase phase = GamePhase.TradeBuild,
        TradeOffer? currentTradeOffer = null,
        TradeOfferId? tradeOfferId = null,
        Player? proposer = null,
        Player? acceptor = null)
    {
        return new AcceptTradeContext(
            phase,
            currentTradeOffer,
            tradeOfferId ?? new TradeOfferId { Value = Guid.NewGuid() },
            proposer,
            acceptor);
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
    public void TradeOfferMustExistAndMatch_OfferWithMatchingId_Succeeds()
    {
        var p = Player.Create("p1");
        p.AddResource(ResourceCardType.Brick, 1);
        var create = TradeOffer.Create(
            p,
            [new ResourceCardAmount(ResourceCardType.Ore, 1)],
            [new ResourceCardAmount(ResourceCardType.Brick, 1)]);
        Assert.True(create.IsSuccess);
        var offer = create.Value;

        var result = new TradeOfferMustExistAndMatch().IsSatisfiedBy(
            MakeContext(currentTradeOffer: offer, tradeOfferId: offer.Id));
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void TradeOfferMustExistAndMatch_NullOffer_Fails()
    {
        var id = new TradeOfferId { Value = Guid.NewGuid() };
        var result = new TradeOfferMustExistAndMatch().IsSatisfiedBy(
            MakeContext(currentTradeOffer: null, tradeOfferId: id));
        Assert.True(result.IsFailure);
        Assert.Equal("NotFound", result.Error.Code);
    }

    [Fact]
    public void BothPlayersMustExist_BothPresent_Succeeds()
    {
        var result = new BothPlayersMustExist().IsSatisfiedBy(
            MakeContext(proposer: Player.Create("p1"), acceptor: Player.Create("p2")));
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void BothPlayersMustExist_ProposerNull_Fails()
    {
        var result = new BothPlayersMustExist().IsSatisfiedBy(
            MakeContext(proposer: null, acceptor: Player.Create("p2")));
        Assert.True(result.IsFailure);
        Assert.Equal("NotFound", result.Error.Code);
    }
}
