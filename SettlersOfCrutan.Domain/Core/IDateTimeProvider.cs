namespace SettlersOfCrutan.Domain.Core;

public interface IDateTimeProvider
{
    DateTimeOffset UtcNow { get; }
}

public sealed class SystemClock : IDateTimeProvider
{
    public SystemClock() { }
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
