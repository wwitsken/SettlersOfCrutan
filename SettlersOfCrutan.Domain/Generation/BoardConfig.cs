using SettlersOfCrutan.Domain.Games;
using SettlersOfCrutan.Domain.Games.Boards;

namespace SettlersOfCrutan.Domain.Generation;
public sealed record BoardConfig
(
    int Radius,
    IReadOnlyDictionary<ResourceType, int> ResourceCounts,
    IReadOnlyDictionary<PortType, int> Ports,
    IReadOnlyList<int> NumberTokens
);
