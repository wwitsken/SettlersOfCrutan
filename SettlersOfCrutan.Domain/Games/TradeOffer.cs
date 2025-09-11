using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.DomainErrors;
using SettlersOfCrutan.Domain.Games.Resources;

namespace SettlersOfCrutan.Domain.Games;
public record TradeOfferId : BaseId<Guid>;
public class TradeOffer : Entity<TradeOfferId>
{
    public override TradeOfferId Id { get; init; } = new() { Value = Guid.NewGuid() };
    public PlayerId ProposerId { get; init; }
    public PlayerId? AcceptorId { get; private set; } = null;
    private List<ResourceCardAmount> _requestedResources = [];
    public List<ResourceCardAmount> RequestedResources { get => [.. _requestedResources]; set => _requestedResources = value; }
    private List<ResourceCardAmount> _offeredResources = [];
    public List<ResourceCardAmount> OfferedResources { get => [.. _offeredResources]; set => _offeredResources = value; }
    public bool IsAccepted => AcceptorId is not null;

    private TradeOffer(PlayerId proposerId, List<ResourceCardAmount> requestedResources, List<ResourceCardAmount> offeredResources)
    {
        ProposerId = proposerId;
        _requestedResources = requestedResources;
        _offeredResources = offeredResources;
    }

    public static Result<TradeOffer> Create(Player proposer, List<ResourceCardAmount> requestedResources, List<ResourceCardAmount> offeredResources)
    {
        if (requestedResources.Count == 0)
            return Result<TradeOffer>.Failure(new DomainError("TradeOffer", "Requested resources cannot be empty"));
        if (offeredResources.Count == 0)
            return Result<TradeOffer>.Failure(new DomainError("TradeOffer", "Offered resources cannot be empty"));
        if (offeredResources.GroupBy(r => r.Type).Any(g => g.Count() > 1))
            return Result<TradeOffer>.Failure(new DomainError("TradeOffer", "Offered resources cannot contain duplicate resource types"));
        if (requestedResources.GroupBy(r => r.Type).Any(g => g.Count() > 1))
            return Result<TradeOffer>.Failure(new DomainError("TradeOffer", "Requested resources cannot contain duplicate resource types"));
        if (!proposer.ResourceHand.CanPay(offeredResources))
            return Result<TradeOffer>.Failure(new DomainError("TradeOffer", "Proposer has insufficient resources to make trade offer"));
        var entity = new TradeOffer(proposer.Id, requestedResources, offeredResources);
        return Result<TradeOffer>.Success(entity);
    }

    // Pure precondition check for acceptance
    public Result<Nothing> CanAccept(Player acceptor, Player originalRequestor)
    {
        if (IsAccepted)
            return Result.Failure<Nothing>(new DomainError("TradeOffer", "Trade offer already accepted"));
        if (acceptor.Id == ProposerId)
            return Result.Failure<Nothing>(new DomainError("TradeOffer", "Proposer cannot accept their own trade offer"));
        if (originalRequestor.Id != ProposerId)
            return Result.Failure<Nothing>(new DomainError("TradeOffer", "Only the original proposer can be the original requestor"));
        if (!acceptor.ResourceHand.CanPay(_requestedResources))
            return Result.Failure<Nothing>(new DomainError("TradeOffer", "Acceptor has insufficient resources to accept trade offer"));
        return Result.Success();
    }

    // No-fail application (assumes CanAccept succeeded)
    public void ApplyTradeNoFail(Player acceptor, Player originalRequestor)
    {
        // Acceptors requested resources flow to proposer
        acceptor.ResourceHand.PayTo(originalRequestor.ResourceHand, _requestedResources);
        // Proposer's offered resources flow to acceptor
        originalRequestor.ResourceHand.PayTo(acceptor.ResourceHand, _offeredResources);
        AcceptorId = acceptor.Id;
    }

    public Result<Nothing> AcceptAndTradeResources(Player acceptor, Player originalRequestor)
    {
        var can = CanAccept(acceptor, originalRequestor);
        if (can.IsFailure) return can;
        ApplyTradeNoFail(acceptor, originalRequestor);
        return Result.Success();
    }
}
