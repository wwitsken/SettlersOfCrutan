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
