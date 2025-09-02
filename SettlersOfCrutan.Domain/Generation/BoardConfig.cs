using SettlersOfCrutan.Domain.Games;

namespace SettlersOfCrutan.Domain.Generation;
public sealed record BoardConfig
(
    int Radius,
    IReadOnlyDictionary<ResourceType, int> ResourceCounts,
    IReadOnlyDictionary<PortType, int> Ports,
    IReadOnlyList<int> NumberTokens
);
