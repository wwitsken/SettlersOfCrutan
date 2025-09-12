using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.DomainErrors;
using SettlersOfCrutan.Domain.Games.Boards;
using SettlersOfCrutan.Domain.Games.DomainEvents;
using SettlersOfCrutan.Domain.Games.Resources;

namespace SettlersOfCrutan.Domain.Games;
public partial class Game
{
    public void ClearTradeOffer() => CurrentTradeOffer = null;

    private Result<Nothing> CanMaritime(PlayerId playerId, int ratio, ResourceCardType discard, ResourceCardType request)
    {
        if (GamePhase != GamePhase.TradeBuild) return Result.Failure<Nothing>(DomainError.WrongGamePhase);
        if (CurrentPlayerId() != playerId) return Result.Failure<Nothing>(DomainError.WrongTurn);
        var player = Players.First(p => p.Id == playerId);
        if (!player.HasAtLeast(new ResourceCardAmount(discard, ratio)))
            return Result.Failure<Nothing>(DomainError.InsufficientResources);
        if (!BankResourceHand.HasAtLeast(new ResourceCardAmount(request, 1)))
            return Result.Failure<Nothing>(DomainError.InsufficientResources);
        if (ratio == 3)
        {
            bool has3to1 = Board.PopulationCenters.Any(pc => pc.PlayerOwner == playerId && Board.Ports.Where(p => p.Type == PortType.Generic3to1).SelectMany(p => p.EdgeCoordinate.HexCoords()).Intersect(pc.VertexCoordinate.HexCoords()).Any());
            if (!has3to1) return Result.Failure<Nothing>(DomainError.Missing3to1Port);
        }
        if (ratio == 2)
        {
            var required = discard switch
            {
                ResourceCardType.Brick => PortType.Brick2to1,
                ResourceCardType.Lumber => PortType.Lumber2to1,
                ResourceCardType.Wool => PortType.Wool2to1,
                ResourceCardType.Grain => PortType.Grain2to1,
                ResourceCardType.Ore => PortType.Ore2to1,
                _ => PortType.None
            };
            bool has2to1 = Board.PopulationCenters.Any(pc => pc.PlayerOwner == playerId && Board.Ports.Where(p => p.Type == required).SelectMany(p => p.EdgeCoordinate.HexCoords()).Intersect(pc.VertexCoordinate.HexCoords()).Any());
            if (!has2to1) return Result.Failure<Nothing>(DomainError.Missing2to1Port);
        }
        return Result.Success();
    }

    private void ApplyMaritimeNoFail(PlayerId playerId, int ratio, ResourceCardType discard, ResourceCardType request)
    {
        var player = Players.First(p => p.Id == playerId);
        player.SubtractResource(discard, ratio); // send discard to bank
        BankResourceHand.Add(discard, ratio);
        BankResourceHand.Transfer(player.ResourceHand, request, 1);     // bank gives requested
        AddDomainEvent(new MaritimeTradeExecutedDomainEvent(Id, playerId, [new ResourceCardAmount(request, 1), new ResourceCardAmount(discard, -ratio)]));
    }

    public Result<Nothing> Maritime4to1Trade(PlayerId playerId, ResourceCardType discardResource, ResourceCardType requestResource)
    {
        var can = CanMaritime(playerId, 4, discardResource, requestResource);
        if (can.IsFailure) return can;
        ApplyMaritimeNoFail(playerId, 4, discardResource, requestResource);
        return Result.Success();
    }

    public Result<Nothing> Maritime3to1Trade(PlayerId playerId, ResourceCardType discardResource, ResourceCardType requestResource)
    {
        var can = CanMaritime(playerId, 3, discardResource, requestResource);
        if (can.IsFailure) return can;
        ApplyMaritimeNoFail(playerId, 3, discardResource, requestResource);
        return Result.Success();
    }

    public Result<Nothing> Maritime2to1Trade(PlayerId playerId, ResourceCardType discardResource, ResourceCardType requestResource)
    {
        var can = CanMaritime(playerId, 2, discardResource, requestResource);
        if (can.IsFailure) return can;
        ApplyMaritimeNoFail(playerId, 2, discardResource, requestResource);
        return Result.Success();
    }

    public Result<Nothing> ProposeTrade(PlayerId playerId, List<ResourceCardAmount> requested, List<ResourceCardAmount> offered)
    {
        if (GamePhase != GamePhase.TradeBuild) return Result.Failure<Nothing>(DomainError.WrongGamePhase);
        if (CurrentPlayerId() != playerId) return Result.Failure<Nothing>(DomainError.WrongTurn);
        if (CurrentTradeOffer is not null && !CurrentTradeOffer.IsAccepted)
            return Result.Failure<Nothing>(DomainError.AnotherTradeOfferActive);

        var proposer = Players.FirstOrDefault(p => p.Id == playerId);
        if (proposer is null) return Result.Failure<Nothing>(DomainError.NotFound);

        if (!proposer.HasAtLeast(offered))
            return Result.Failure<Nothing>(DomainError.InsufficientResources);

        var create = TradeOffer.Create(proposer, requested, offered);
        if (create.IsFailure) return Result.Failure<Nothing>(create.Error);

        CurrentTradeOffer = create.Value;
        AddDomainEvent(new TradeOfferPostedDomainEvent(Id, CurrentTradeOffer.Id, playerId, requested, offered));
        return Result.Success();
    }

    public Result<Nothing> AcceptTrade(PlayerId playerId, TradeOfferId tradeOfferId)
    {
        if (GamePhase != GamePhase.TradeBuild) return Result.Failure<Nothing>(DomainError.WrongGamePhase);
        if (CurrentTradeOffer is null || CurrentTradeOffer.Id != tradeOfferId)
            return Result.Failure<Nothing>(DomainError.NotFound);

        var proposer = Players.FirstOrDefault(p => p.Id == CurrentTradeOffer.ProposerId);
        var acceptor = Players.FirstOrDefault(p => p.Id == playerId);
        if (proposer is null || acceptor is null) return Result.Failure<Nothing>(DomainError.NotFound);

        var can = CurrentTradeOffer.CanAccept(acceptor, proposer);
        if (can.IsFailure) return can;

        CurrentTradeOffer.ApplyTradeNoFail(acceptor, proposer);

        AddDomainEvent(new TradeExecutedDomainEvent(
            Id,
            CurrentTradeOffer.Id,
            proposer.Id,
            acceptor.Id,
            CurrentTradeOffer.OfferedResources,
            CurrentTradeOffer.RequestedResources));

        ClearTradeOffer();
        return Result.Success();
    }
}
