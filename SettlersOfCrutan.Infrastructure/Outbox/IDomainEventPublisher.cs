using SettlersOfCrutan.Domain.Core;

namespace SettlersOfCrutan.Infrastructure.Outbox;
public interface IDomainEventPublisher
{
    Task PublishAsync(IDomainEvent domainEvent, CancellationToken ct = default);
}
