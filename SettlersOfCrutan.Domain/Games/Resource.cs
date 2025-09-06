namespace SettlersOfCrutan.Domain.Games;

public enum ResourceType
{
    None = 0,
    Brick,
    Lumber,
    Wool,
    Grain,
    Ore,
    Desert
}

public record ResourceAmount(ResourceType Type, int Quantity);

public static class ResourceAmountExtensions
{
    public static IEnumerable<ResourceAmount> WherePositive(this IEnumerable<ResourceAmount> resourceAmounts) =>
        resourceAmounts.Where(ra => ra.Quantity > 0);

    public static IEnumerable<ResourceAmount> Invert(this IEnumerable<ResourceAmount> resourceAmounts) =>
        resourceAmounts.Select(ra => new ResourceAmount(ra.Type, -ra.Quantity));
}
