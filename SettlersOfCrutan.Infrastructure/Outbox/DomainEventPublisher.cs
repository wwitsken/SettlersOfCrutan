using SettlersOfCrutan.Application.Abstractions;
using SettlersOfCrutan.Domain.Core;
using System.Collections;

namespace SettlersOfCrutan.Infrastructure.Outbox;
public class DomainEventPublisher(IServiceProvider serviceProvider) : IDomainEventPublisher
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    public async Task PublishAsync(IDomainEvent domainEvent, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);

        var eventType = domainEvent.GetType();
        var handlerInterfaceType = typeof(IDomainEventHandler<>).MakeGenericType(eventType);
        var enumerableHandlerType = typeof(IEnumerable<>).MakeGenericType(handlerInterfaceType);

        if (_serviceProvider.GetService(enumerableHandlerType) is not IEnumerable handlersObj)
            return; // no handlers registered for this event type

        var handleAsyncMethod = handlerInterfaceType.GetMethod(
            name: nameof(IDomainEventHandler<IDomainEvent>.HandleAsync),
            types: [eventType, typeof(CancellationToken)]
        );

        if (handleAsyncMethod is null)
            return;

        foreach (var handler in handlersObj)
        {
            // Each handler has signature Task<Result<TEvent>>> HandleAsync(TEvent, CancellationToken)
            var taskObj = handleAsyncMethod.Invoke(handler, [domainEvent, ct]);
            if (taskObj is Task task)
            {
                await task;
            }
        }
    }
}
