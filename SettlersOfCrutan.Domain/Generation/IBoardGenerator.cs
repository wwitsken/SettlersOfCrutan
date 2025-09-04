using SettlersOfCrutan.Domain.Games.Boards;

namespace SettlersOfCrutan.Domain.Generation;
public interface IBoardGenerator
{
    Board Generate(BoardConfig config, int seed);
}