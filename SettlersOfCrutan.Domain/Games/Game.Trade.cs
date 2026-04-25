using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.Games.DomainEvents;
using SettlersOfCrutan.Domain.Games.Resources;
using SettlersOfCrutan.Domain.Specifications;
using M4Specs = SettlersOfCrutan.Domain.Specifications.Maritime4to1Trade;
using M3Specs = SettlersOfCrutan.Domain.Specifications.Maritime3to1Trade;
using M2Specs = SettlersOfCrutan.Domain.Specifications.Maritime2to1Trade;
using ProposeSpecs = SettlersOfCrutan.Domain.Specifications.ProposeTrade;
using AcceptSpecs = SettlersOfCrutan.Domain.Specifications.AcceptTrade;

namespace SettlersOfCrutan.Domain.Games;
public partial class Game
{
    public void ClearTradeOffer() => CurrentTradeOffer = null;

    private void ApplyMaritimeNoFail(PlayerId playerId, int ratio, ResourceCardType discard, ResourceCardType request)
    {
        var player = Players.First(p => p.Id == playerId);
        player.SubtractResource(discard, ratio);
        BankResourceHand.Add(discard, ratio);
        BankResourceHand.Transfer(player.ResourceHand, request, 1);
        AddDomainEvent(new MaritimeTradeExecutedDomainEvent(Id, playerId, [new ResourceCardAmount(request, 1), new ResourceCardAmount(discard, -ratio)]));
    }

    private static readonly ISpecification<M4Specs.Maritime4to1TradeContext>[] Maritime4to1TradeSpecifications =
    [
        new M4Specs.GameMustBeInTradeBuildPhase(),
        new M4Specs.MustBeCurrentPlayerTurn(),
        new M4Specs.PlayerMustHaveDiscardResources(),
        new M4Specs.BankMustHaveRequestedResource()
    ];

    public Result<Nothing> Maritime4to1Trade(PlayerId playerId, ResourceCardType discardResource, ResourceCardType requestResource)
    {
        var player = Players.First(p => p.Id == playerId);
        var context = new M4Specs.Maritime4to1TradeContext(
            GamePhase, CurrentPlayerId(), playerId, player, discardResource, requestResource, BankResourceHand);

        foreach (var spec in Maritime4to1TradeSpecifications)
        {
            var result = spec.IsSatisfiedBy(context);
            if (result.IsFailure) return result;
        }

        ApplyMaritimeNoFail(playerId, 4, discardResource, requestResource);
        return Result.Success();
    }

    private static readonly ISpecification<M3Specs.Maritime3to1TradeContext>[] Maritime3to1TradeSpecifications =
    [
        new M3Specs.GameMustBeInTradeBuildPhase(),
        new M3Specs.MustBeCurrentPlayerTurn(),
        new M3Specs.PlayerMustHaveDiscardResources(),
        new M3Specs.BankMustHaveRequestedResource(),
        new M3Specs.PlayerMustHave3to1Port()
    ];

    public Result<Nothing> Maritime3to1Trade(PlayerId playerId, ResourceCardType discardResource, ResourceCardType requestResource)
    {
        var player = Players.First(p => p.Id == playerId);
        var context = new M3Specs.Maritime3to1TradeContext(
            GamePhase, CurrentPlayerId(), playerId, player, discardResource, requestResource, BankResourceHand, Board);

        foreach (var spec in Maritime3to1TradeSpecifications)
        {
            var result = spec.IsSatisfiedBy(context);
            if (result.IsFailure) return result;
        }

        ApplyMaritimeNoFail(playerId, 3, discardResource, requestResource);
        return Result.Success();
    }

    private static readonly ISpecification<M2Specs.Maritime2to1TradeContext>[] Maritime2to1TradeSpecifications =
    [
        new M2Specs.GameMustBeInTradeBuildPhase(),
        new M2Specs.MustBeCurrentPlayerTurn(),
        new M2Specs.PlayerMustHaveDiscardResources(),
        new M2Specs.BankMustHaveRequestedResource(),
        new M2Specs.PlayerMustHaveMatching2to1Port()
    ];

    public Result<Nothing> Maritime2to1Trade(PlayerId playerId, ResourceCardType discardResource, ResourceCardType requestResource)
    {
        var player = Players.First(p => p.Id == playerId);
        var context = new M2Specs.Maritime2to1TradeContext(
            GamePhase, CurrentPlayerId(), playerId, player, discardResource, requestResource, BankResourceHand, Board);

        foreach (var spec in Maritime2to1TradeSpecifications)
        {
            var result = spec.IsSatisfiedBy(context);
            if (result.IsFailure) return result;
        }

        ApplyMaritimeNoFail(playerId, 2, discardResource, requestResource);
        return Result.Success();
    }

    private static readonly ISpecification<ProposeSpecs.ProposeTradeContext>[] ProposeTradeSpecifications =
    [
        new ProposeSpecs.GameMustBeInTradeBuildPhase(),
        new ProposeSpecs.MustBeCurrentPlayerTurn(),
        new ProposeSpecs.NoActiveTradeOffer(),
        new ProposeSpecs.ProposerMustExist(),
        new ProposeSpecs.ProposerMustHaveResources()
    ];

    public Result<Nothing> ProposeTrade(PlayerId playerId, List<ResourceCardAmount> requested, List<ResourceCardAmount> offered)
    {
        var proposer = Players.FirstOrDefault(p => p.Id == playerId);
        var context = new ProposeSpecs.ProposeTradeContext(
            GamePhase, CurrentPlayerId(), playerId, CurrentTradeOffer, proposer, offered);

        foreach (var spec in ProposeTradeSpecifications)
        {
            var result = spec.IsSatisfiedBy(context);
            if (result.IsFailure) return result;
        }

        var create = TradeOffer.Create(proposer!, requested, offered);
        if (create.IsFailure) return Result.Failure<Nothing>(create.Error);

        CurrentTradeOffer = create.Value;
        AddDomainEvent(new TradeOfferPostedDomainEvent(Id, CurrentTradeOffer.Id, playerId, requested, offered));
        return Result.Success();
    }

    private static readonly ISpecification<AcceptSpecs.AcceptTradeContext>[] AcceptTradeSpecifications =
    [
        new AcceptSpecs.GameMustBeInTradeBuildPhase(),
        new AcceptSpecs.TradeOfferMustExistAndMatch(),
        new AcceptSpecs.BothPlayersMustExist()
    ];

    public Result<Nothing> AcceptTrade(PlayerId playerId, TradeOfferId tradeOfferId)
    {
        var proposer = CurrentTradeOffer is not null
            ? Players.FirstOrDefault(p => p.Id == CurrentTradeOffer.ProposerId)
            : null;
        var acceptor = Players.FirstOrDefault(p => p.Id == playerId);
        var context = new AcceptSpecs.AcceptTradeContext(
            GamePhase, CurrentTradeOffer, tradeOfferId, proposer, acceptor);

        foreach (var spec in AcceptTradeSpecifications)
        {
            var result = spec.IsSatisfiedBy(context);
            if (result.IsFailure) return result;
        }

        var can = CurrentTradeOffer!.CanAccept(acceptor!, proposer!);
        if (can.IsFailure) return can;

        CurrentTradeOffer.ApplyTradeNoFail(acceptor!, proposer!);

        AddDomainEvent(new TradeExecutedDomainEvent(
            Id,
            CurrentTradeOffer.Id,
            proposer!.Id,
            acceptor!.Id,
            CurrentTradeOffer.OfferedResources,
            CurrentTradeOffer.RequestedResources));

        ClearTradeOffer();
        return Result.Success();
    }
}
