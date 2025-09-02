using SettlersOfCrutan.Domain.Core;
using StackExchange.Redis;

namespace SettlersOfCrutan.Infrastructure.Redis;
public static class RedisKeys
{
    public static RedisKey Aggregate<TId>
        (string prefix, string aggName, TId id)
        where TId : BaseId
        => $"{prefix}:{aggName}:{id}";

    public static RedisKey Outbox(string prefix)
        => $"{prefix}:outbox"; // Global outbox stream for domain events

    public static RedisKey OutboxPoison(string prefix)
        => $"{prefix}:outbox:poison"; // dead-letter list (or stream)

    public static RedisKey OutboxAttemptsHash(string prefix)
        => $"{prefix}:outbox:attempts"; // hash to track retry counts per message-id
}