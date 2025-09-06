using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.DomainErrors;

namespace SettlersOfCrutan.Domain.Games;
public record TradeOfferId : BaseId<Guid>;
public class TradeOffer : Entity<TradeOfferId>
{
    public override TradeOfferId Id { get; init; } = new() { Value = Guid.NewGuid() };
    public PlayerId ProposerId { get; init; }
    public PlayerId? AcceptorId { get; private set; } = null; // null if not yet accepted
    private List<ResourceAmount> _requestedResources = [];
    public List<ResourceAmount> RequestedResources
    {
        get => [.. _requestedResources];
        set => _requestedResources = value;
    }
    private List<ResourceAmount> _offeredResources = [];
    public List<ResourceAmount> OfferedResources
    {
        get => [.. _offeredResources];
        set => _offeredResources = value;
    }
    public bool IsAccepted => AcceptorId is not null;
    //private TradeOffer() { } // for deserialization
    private TradeOffer(PlayerId proposerId, List<ResourceAmount> requestedResources, List<ResourceAmount> offeredResources)
    {
        ProposerId = proposerId;
        _requestedResources = requestedResources;
        _offeredResources = offeredResources;
    }
    public static Result<TradeOffer> Create(Player proposer, List<ResourceAmount> requestedResources, List<ResourceAmount> offeredResources)
    {
        if (requestedResources.Count == 0)
            return Result<TradeOffer>.Failure(new DomainError("TradeOffer", "Requested resources cannot be empty"));

        if (offeredResources.Count == 0)
            return Result<TradeOffer>.Failure(new DomainError("TradeOffer", "Offered resources cannot be empty"));

        if (offeredResources.GroupBy(r => r.Type).Any(g => g.Count() > 1))
            return Result<TradeOffer>.Failure(new DomainError("TradeOffer", "Offered resources cannot contain duplicate resource types"));

        if (requestedResources.GroupBy(r => r.Type).Any(g => g.Count() > 1))
            return Result<TradeOffer>.Failure(new DomainError("TradeOffer", "Requested resources cannot contain duplicate resource types"));

        var hasEnough = offeredResources.All(r => proposer.ResourceBag.HasAtLeast(r.Type, r.Quantity));

        if (!hasEnough) return Result<TradeOffer>.Failure(new DomainError("TradeOffer", "Proposer has insufficient resources to make trade offer"));

        var entity = new TradeOffer(proposer.Id, requestedResources, offeredResources);

        return Result<TradeOffer>.Success(entity);
    }
    public Result<Nothing> AcceptAndTradeResources(Player acceptor, Player originalRequestor)
    {
        if (IsAccepted)
            return Result<Nothing>.Failure(new DomainError("TradeOffer", "Trade offer already accepted"));

        if (acceptor.Id == ProposerId)
            return Result<Nothing>.Failure(new DomainError("TradeOffer", "Proposer cannot accept their own trade offer"));

        if (originalRequestor.Id != ProposerId)
            return Result<Nothing>.Failure(new DomainError("TradeOffer", "Only the original proposer can be the original requestor"));

        var hasEnough = _requestedResources.All(r => acceptor.ResourceBag.HasAtLeast(r.Type, r.Quantity));

        if (!hasEnough) return Result<Nothing>.Failure(new DomainError("TradeOffer", "Acceptor has insufficient resources to accept trade offer"));

        foreach (var req in _requestedResources)
        {
            acceptor.ResourceBag.SubtractResource(req.Type, req.Quantity);
            originalRequestor.ResourceBag.AddResource(req.Type, req.Quantity);
        }

        foreach (var off in _offeredResources)
        {
            originalRequestor.ResourceBag.SubtractResource(off.Type, off.Quantity);
            acceptor.ResourceBag.AddResource(off.Type, off.Quantity);
        }

        AcceptorId = acceptor.Id;

        return Result.Success();
    }
}
