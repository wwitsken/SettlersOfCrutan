using SettlersOfCrutan.Domain.Games;

namespace SettlersOfCrutan.Domain.Generation;
public static class StandardBoardConfigurations
{
    public static BoardConfig DefaultBaseGame => new
        (
        Radius: 2,
        ResourceCounts: new Dictionary<ResourceType, int>
        {
            [ResourceType.Brick] = 3,
            [ResourceType.Lumber] = 4,
            [ResourceType.Wool] = 4,
            [ResourceType.Grain] = 4,
            [ResourceType.Ore] = 3,
            [ResourceType.Desert] = 1,
        },
        NumberTokens: [2, 3, 3, 4, 4, 5, 5, 6, 6, 8, 8, 9, 9, 10, 10, 11, 11, 12],
        Ports: new Dictionary<PortType, int>
        {
            [PortType.Generic3to1] = 4,
            [PortType.Brick2to1] = 1,
            [PortType.Lumber2to1] = 1,
            [PortType.Wool2to1] = 1,
            [PortType.Grain2to1] = 1,
            [PortType.Ore2to1] = 1
        });
}
