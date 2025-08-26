using SettlersOfCrutan.Domain.Games;

namespace SettlersOfCrutan.Domain.Generation;
public sealed record BoardConfig
(
    int Radius,
    IReadOnlyDictionary<ResourceType, int> ResourceCounts,
    IReadOnlyList<int> NumberTokens,
    IReadOnlyList<(PortType type, int count)> Ports
);
