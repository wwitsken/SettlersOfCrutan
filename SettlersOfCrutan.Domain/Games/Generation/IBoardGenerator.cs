using SettlersOfCrutan.Domain.Games.Boards;

namespace SettlersOfCrutan.Domain.Games.Generation;
public interface IBoardGenerator
{
    Board Generate(BoardConfig config, int seed);
}