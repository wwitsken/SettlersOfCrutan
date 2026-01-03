using Microsoft.Extensions.Logging;
using SettlersOfCrutan.Application.Abstractions;
using SettlersOfCrutan.Application.Lobbies.DTOs;
using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.DomainErrors;
using SettlersOfCrutan.Domain.Games.DomainEvents;
using SettlersOfCrutan.Domain.Lobbies;

namespace SettlersOfCrutan.Application.Lobbies.DomainEventHandlers;


public sealed class GameCreatedFromLobbyDomainEventHandler(IRealtimePublisher realtimePublisher, ILobbyRepository lobbyRepository, ILogger<GameCreatedFromLobbyDomainEventHandler> logger)
    : IDomainEventHandler<GameCreatedFromLobbyDomainEvent>
{
    private readonly ILobbyRepository _lobbyRepository = lobbyRepository;
    private readonly IRealtimePublisher _realtimePublisher = realtimePublisher;
    private readonly ILogger<GameCreatedFromLobbyDomainEventHandler> _logger = logger;

    public async Task<Result<GameCreatedFromLobbyDomainEvent>> HandleAsync(GameCreatedFromLobbyDomainEvent domainEvent, CancellationToken ct = default)
    {
        Lobby? lobby = await _lobbyRepository.GetAsync(domainEvent.SpawnerLobbyId, ct);
        if (lobby is null) return Result.Failure<GameCreatedFromLobbyDomainEvent>(DomainError.NotFound);
        await _realtimePublisher.MoveFromLobbyToGameAsync(lobby.Id, domainEvent.GameId, domainEvent.PlayerUserIds, DateTimeOffset.Now, ct);
        await _lobbyRepository.DeleteAsync(lobby.Id, ct);

        return Result<GameCreatedFromLobbyDomainEvent>.Success(domainEvent);
    }
}