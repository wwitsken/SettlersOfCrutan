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
            return Result<TradeOffer>.Failure(DomainError.RequestedResourcesEmpty);
        if (offeredResources.Count == 0)
            return Result<TradeOffer>.Failure(DomainError.OfferedResourcesEmpty);
        if (offeredResources.GroupBy(r => r.Type).Any(g => g.Count() > 1))
            return Result<TradeOffer>.Failure(DomainError.OfferedResourcesDuplicateTypes);
        if (requestedResources.GroupBy(r => r.Type).Any(g => g.Count() > 1))
            return Result<TradeOffer>.Failure(DomainError.RequestedResourcesDuplicateTypes);
        if (!proposer.ResourceHand.CanPay(offeredResources))
            return Result<TradeOffer>.Failure(DomainError.ProposerInsufficientResources);
        var entity = new TradeOffer(proposer.Id, requestedResources, offeredResources);
        return Result<TradeOffer>.Success(entity);
    }

    // Pure precondition check for acceptance
    public Result<Nothing> CanAccept(Player acceptor, Player originalRequestor)
    {
        if (IsAccepted)
            return Result.Failure<Nothing>(DomainError.TradeOfferAlreadyAccepted);
        if (acceptor.Id == ProposerId)
            return Result.Failure<Nothing>(DomainError.ProposerCannotAcceptOwnOffer);
        if (originalRequestor.Id != ProposerId)
            return Result.Failure<Nothing>(DomainError.InvalidOriginalRequestor);
        if (!acceptor.ResourceHand.CanPay(_requestedResources))
            return Result.Failure<Nothing>(DomainError.AcceptorInsufficientResources);
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
