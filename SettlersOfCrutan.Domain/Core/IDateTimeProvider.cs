namespace SettlersOfCrutan.Domain.Core;

public interface IDateTimeProvider
{
    DateTimeOffset UtcNow { get; }
}