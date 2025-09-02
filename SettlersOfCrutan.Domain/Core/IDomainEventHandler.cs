namespace SettlersOfCrutan.Domain.Core;
public interface IDomainEventHandler<TEvent> where TEvent : IDomainEvent
{
    Task<Result<TEvent>> HandleAsync(TEvent domainEvent, CancellationToken ct = default);
}
