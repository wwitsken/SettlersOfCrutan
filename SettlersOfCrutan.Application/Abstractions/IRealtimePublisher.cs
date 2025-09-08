namespace SettlersOfCrutan.Application.Abstractions;
public interface IRealtimePublisher
{
    Task PublishAsync<TMessage>(string channel, TMessage message, CancellationToken ct = default);
    Task PublishToUserAsync<TMessage>(string userId, TMessage message, CancellationToken ct = default);
    Task PublishToGroupAsync<TMessage>(string group, TMessage message, CancellationToken ct = default);
}
