using SettlersOfCrutan.Application.Abstractions;
using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.DomainErrors;
using SettlersOfCrutan.Domain.Games.DomainEvents;
using SettlersOfCrutan.Domain.Lobbies;

namespace SettlersOfCrutan.Application.Lobbies.DomainEventHandlers;
public sealed class GameCreatedFromLobbyDomainEventHandler(ILobbyRepository lobbyRepository)
    : IDomainEventHandler<GameCreatedFromLobbyDomainEvent>
{
    private readonly ILobbyRepository _lobbyRepository = lobbyRepository;

    public async Task<Result<GameCreatedFromLobbyDomainEvent>> HandleAsync(GameCreatedFromLobbyDomainEvent domainEvent, CancellationToken ct = default)
    {
        Lobby? lobby = await _lobbyRepository.GetAsync(domainEvent.SpawnerLobbyId, ct);
        if (lobby is null) return Result.Failure<GameCreatedFromLobbyDomainEvent>(DomainError.NotFound);
        await _lobbyRepository.DeleteAsync(lobby.Id, ct);

        return Result<GameCreatedFromLobbyDomainEvent>.Success(domainEvent);
    }
}
