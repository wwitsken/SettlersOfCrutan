using SettlersOfCrutan.Domain.PlayerPresence;

namespace SettlersOfCrutan.Application.Abstractions;
public interface IPlayerPresenceRepository : IRepository<PlayerPresence, PlayerPresenceId>;