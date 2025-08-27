using SettlersOfCrutan.Application.Abstractions;
using SettlersOfCrutan.Domain.Games;

namespace SettlersOfCrutan.Application.Games;
public interface IGameRepository : IRepository<Game, GameId>;