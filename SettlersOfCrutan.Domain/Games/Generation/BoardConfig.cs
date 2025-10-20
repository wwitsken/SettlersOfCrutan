using SettlersOfCrutan.Domain.Games.Boards;
using SettlersOfCrutan.Domain.Games.Resources;

namespace SettlersOfCrutan.Domain.Games.Generation;
public sealed record BoardConfig
(
    int Radius,
    IReadOnlyDictionary<ResourceCardType, int> ResourceCounts,
    IReadOnlyDictionary<PortType, int> Ports,
    IReadOnlyList<int> NumberTokens
);
