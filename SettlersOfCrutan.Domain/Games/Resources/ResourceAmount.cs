namespace SettlersOfCrutan.Domain.Games.Resources;
public record ResourceAmount<TResource>(TResource Type, int Quantity)
    where TResource : Enum;

public static class ResourceAmountExtensions
{
    public static IEnumerable<ResourceAmount<TResource>> Invert<TResource>(this IEnumerable<ResourceAmount<TResource>> resourceAmounts)
        where TResource : Enum => resourceAmounts.Select(ra => new ResourceAmount<TResource>(ra.Type, -ra.Quantity));
    public static ResourceAmount<TResource> Invert<TResource>(this ResourceAmount<TResource> resourceAmount)
        where TResource : Enum => new(resourceAmount.Type, -resourceAmount.Quantity);
}