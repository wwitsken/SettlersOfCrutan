using SettlersOfCrutan.Domain.Games;

namespace SettlersOfCrutan.Application.Abstractions;
public interface IGameRepository : IRepository<Game, GameId>;