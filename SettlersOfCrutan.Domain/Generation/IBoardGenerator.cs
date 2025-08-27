using SettlersOfCrutan.Domain.Games;

namespace SettlersOfCrutan.Domain.Generation;
public interface IBoardGenerator
{
    Board Generate(BoardConfig config, int seed);


}