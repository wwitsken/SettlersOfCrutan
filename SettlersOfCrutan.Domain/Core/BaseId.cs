namespace SettlersOfCrutan.Domain.Core;
public abstract record BaseId
{
    public Guid Value { get; set; }
}
