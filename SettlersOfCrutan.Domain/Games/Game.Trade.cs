using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.Games.Boards;
using SettlersOfCrutan.Domain.Games.DomainEvents;

namespace SettlersOfCrutan.Domain.Games;
public partial class Game
{
    public void ClearTradeOffer() => CurrentTradeOffer = null;
    public Result<Nothing> Maritime4to1Trade(PlayerId playerId, ResourceType discardResource, ResourceType requestResource)
    {
        if (CurrentPlayerId() != playerId) return Result<Nothing>.Failure(DomainErrors.DomainError.WrongTurn);
        if (GamePhase != GamePhase.TradeBuild) return Result<Nothing>.Failure(DomainErrors.DomainError.WrongGamePhase);
        return BaseMaritimeTrade(playerId, discardResource, requestResource, 4);
    }

    public Result<Nothing> Maritime3to1Trade(PlayerId playerId, ResourceType discardResource, ResourceType requestResource)
    {
        if (CurrentPlayerId() != playerId) return Result<Nothing>.Failure(DomainErrors.DomainError.WrongTurn);
        if (GamePhase != GamePhase.TradeBuild) return Result<Nothing>.Failure(DomainErrors.DomainError.WrongGamePhase);
        if (!Board.PopulationCenters.Any(pc =>
                pc.PlayerOwner == playerId &&
                Board.Ports
                    .Where(p => p.Type == PortType.Generic3to1)
                    .SelectMany(p => p.EdgeCoordinate.HexCoords())
                    .Intersect(pc.VertexCoordinate.HexCoords())
                    .Any()
            ))
            return Result<Nothing>.Failure(DomainErrors.DomainError.Missing3to1Port);

        return BaseMaritimeTrade(playerId, discardResource, requestResource, 3);
    }

    public Result<Nothing> Maritime2to1Trade(PlayerId playerId, ResourceType discardResource, ResourceType requestResource)
    {
        if (CurrentPlayerId() != playerId) return Result<Nothing>.Failure(DomainErrors.DomainError.WrongTurn);
        if (GamePhase != GamePhase.TradeBuild) return Result<Nothing>.Failure(DomainErrors.DomainError.WrongGamePhase);

        var requiredPortType = discardResource switch
        {
            ResourceType.Brick => PortType.Brick2to1,
            ResourceType.Lumber => PortType.Lumber2to1,
            ResourceType.Wool => PortType.Wool2to1,
            ResourceType.Grain => PortType.Grain2to1,
            ResourceType.Ore => PortType.Ore2to1,
            _ => PortType.None
        };

        if (!Board.PopulationCenters.Any(pc =>
        pc.PlayerOwner == playerId &&
            Board.Ports
                .Where(p => p.Type == requiredPortType)
                .SelectMany(p => p.EdgeCoordinate.HexCoords())
                .Intersect(pc.VertexCoordinate.HexCoords())
                .Any()
            ))
            return Result<Nothing>.Failure(DomainErrors.DomainError.Missing2to1Port);

        return BaseMaritimeTrade(playerId, discardResource, requestResource, 2);
    }

    public Result<Nothing> ProposeTrade(PlayerId playerId, List<ResourceAmount> requested, List<ResourceAmount> offered)
    {
        if (CurrentPlayerId() != playerId) return Result<Nothing>.Failure(DomainErrors.DomainError.WrongTurn);
        if (GamePhase != GamePhase.TradeBuild) return Result<Nothing>.Failure(DomainErrors.DomainError.WrongGamePhase);
        if (CurrentTradeOffer is not null && !CurrentTradeOffer.IsAccepted)
            return Result<Nothing>.Failure(new Error("TradeOffer", "Another trade offer is already active"));

        var proposer = Players.FirstOrDefault(p => p.Id == playerId);
        if (proposer is null) return Result<Nothing>.Failure(DomainErrors.DomainError.NotFound);

        var create = TradeOffer.Create(proposer, requested, offered);
        if (create.IsFailure) return Result<Nothing>.Failure(create.Error);

        CurrentTradeOffer = create.Value;
        AddDomainEvent(new TradeOfferPostedDomainEvent(Id, CurrentTradeOffer.Id, playerId, requested, offered));
        return Result.Success();
    }

    public Result<Nothing> AcceptTrade(PlayerId playerId, TradeOfferId tradeOfferId)
    {
        if (GamePhase != GamePhase.TradeBuild) return Result<Nothing>.Failure(DomainErrors.DomainError.WrongGamePhase);
        if (CurrentTradeOffer is null || CurrentTradeOffer.Id != tradeOfferId)
            return Result<Nothing>.Failure(DomainErrors.DomainError.NotFound);

        var proposer = Players.FirstOrDefault(p => p.Id == CurrentTradeOffer.ProposerId);
        var acceptor = Players.FirstOrDefault(p => p.Id == playerId);
        if (proposer is null || acceptor is null) return Result<Nothing>.Failure(DomainErrors.DomainError.NotFound);

        var accept = CurrentTradeOffer.AcceptAndTradeResources(acceptor, proposer);
        if (accept.IsFailure) return accept;

        AddDomainEvent(new TradeExecutedDomainEvent(
            Id,
            CurrentTradeOffer.Id,
            proposer.Id,
            acceptor.Id,
            CurrentTradeOffer.OfferedResources,
            CurrentTradeOffer.RequestedResources));

        // Clear the active offer after a successful trade
        ClearTradeOffer();
        return Result.Success();
    }

    private Result<Nothing> BaseMaritimeTrade(PlayerId playerId, ResourceType discardResource, ResourceType requestResource, int discardAmount)
    {
        var player = Players.First(p => p.Id == playerId);
        var playerOffer = new ResourceAmount(discardResource, discardAmount);
        var bankOffer = new ResourceAmount(requestResource, 1);

        if (!CanTrade(player.ResourceBag, Bank, playerOffer, bankOffer))
            return Result<Nothing>.Failure(DomainErrors.DomainError.InsufficientResources);

        player.ResourceBag.ApplyResourceAmounts([playerOffer.Invert(), bankOffer]);
        Bank.ApplyResourceAmounts([playerOffer, bankOffer.Invert()]);

        AddDomainEvent(new MaritimeTradeExecutedDomainEvent(Id, playerId, [bankOffer, playerOffer.Invert()]));

        return Result.Success();
    }
    private static bool CanTrade(ResourceBag playerBag, ResourceBag otherBag, ResourceAmount playerOfferAmount, ResourceAmount otherOfferAmount)
    {
        return playerBag.HasAtLeast(playerOfferAmount.Type, playerOfferAmount.Quantity) &&
               otherBag.HasAtLeast(otherOfferAmount.Type, otherOfferAmount.Quantity);
    }
}
