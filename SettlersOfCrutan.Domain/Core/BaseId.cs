namespace SettlersOfCrutan.Domain.Core;

// Base non-generic marker type for IDs
public abstract record BaseId;

// Generic BaseId that can hold any value type (e.g., Guid, custom value object)
public abstract record BaseId<TValue>
: BaseId
{
    public required TValue Value { get; init; }

    public override string ToString() => Value?.ToString() ?? string.Empty;

}