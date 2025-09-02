namespace SettlersOfCrutan.Infrastructure.Redis;
public sealed class RedisOptions
{
    public string KeyPrefix { get; init; } = "crutanapp";     // e.g. "crutanapp:prod"
    public TimeSpan? DefaultTtl { get; init; }            // optional per-aggregate TTL
    public bool UseRedisJson { get; init; } = false;      // toggle JSON module
}