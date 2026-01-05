using SettlersOfCrutan.Domain.Core;

namespace SettlersOfCrutan.Infrastructure.Clock;

public class SystemClock : IDateTimeProvider
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
