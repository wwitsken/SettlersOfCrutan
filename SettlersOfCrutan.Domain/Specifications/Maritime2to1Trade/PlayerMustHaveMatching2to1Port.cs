using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.Games.Boards;
using SettlersOfCrutan.Domain.Games.Resources;

namespace SettlersOfCrutan.Domain.Specifications.Maritime2to1Trade;

public class PlayerMustHaveMatching2to1Port : ISpecification<Maritime2to1TradeContext>
{
    public Result<Nothing> IsSatisfiedBy(Maritime2to1TradeContext context)
    {
        var required = context.DiscardResource switch
        {
            ResourceCardType.Brick => PortType.Brick2to1,
            ResourceCardType.Lumber => PortType.Lumber2to1,
            ResourceCardType.Wool => PortType.Wool2to1,
            ResourceCardType.Grain => PortType.Grain2to1,
            ResourceCardType.Ore => PortType.Ore2to1,
            _ => PortType.None
        };

        bool has2to1 = context.Board.PopulationCenters.Any(pc =>
            pc.OwnerId == context.ActingPlayerId &&
            context.Board.Ports.Any(p =>
                p.Type == required && PortVertexAdjacency.PopulationCenterTouchesPort(p, pc)));

        return has2to1 ? Result.Success() : Result.Failure(DomainError.Missing2to1Port);
    }
}
