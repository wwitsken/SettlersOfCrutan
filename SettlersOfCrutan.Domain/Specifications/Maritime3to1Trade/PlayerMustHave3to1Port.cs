using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.DomainErrors;
using SettlersOfCrutan.Domain.Games.Boards;

namespace SettlersOfCrutan.Domain.Specifications.Maritime3to1Trade;

public class PlayerMustHave3to1Port : ISpecification<Maritime3to1TradeContext>
{
    public Result<Nothing> IsSatisfiedBy(Maritime3to1TradeContext context)
    {
        bool has3to1 = context.Board.PopulationCenters.Any(pc =>
            pc.PlayerOwner == context.ActingPlayerId &&
            context.Board.Ports
                .Where(p => p.Type == PortType.Generic3to1)
                .SelectMany(p => p.EdgeCoordinate.HexCoords())
                .Intersect(pc.VertexCoordinate.HexCoords())
                .Any());

        return has3to1 ? Result.Success() : Result.Failure(DomainError.Missing3to1Port);
    }
}
