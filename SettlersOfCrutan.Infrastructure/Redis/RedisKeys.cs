using SettlersOfCrutan.Domain.Core;
using StackExchange.Redis;

namespace SettlersOfCrutan.Infrastructure.Redis;
public static class RedisKeys
{
    public static RedisKey Aggregate<TId>(string prefix, string aggName, TId id)
        where TId : BaseId
        => $"{prefix}:{aggName}:{id}";
}
