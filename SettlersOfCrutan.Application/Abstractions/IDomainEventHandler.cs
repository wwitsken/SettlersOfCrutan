using SettlersOfCrutan.Domain.Core;

namespace SettlersOfCrutan.Application.Abstractions;
public interface IDomainEventHandler<TEvent> where TEvent : IDomainEvent
{
    Task<Result<TEvent>> HandleAsync(TEvent domainEvent, CancellationToken ct = default);
}
