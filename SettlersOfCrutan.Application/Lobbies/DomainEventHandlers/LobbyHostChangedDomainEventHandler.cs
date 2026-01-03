using Microsoft.Extensions.Logging;
using SettlersOfCrutan.Application.Abstractions;
using SettlersOfCrutan.Application.Lobbies.DTOs;
using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.DomainErrors;
using SettlersOfCrutan.Domain.Lobbies;
using SettlersOfCrutan.Domain.Lobbies.DomainEvents;

namespace SettlersOfCrutan.Application.Lobbies.DomainEventHandlers;

public sealed class LobbyHostChangedDomainEventHandler(IRealtimePublisher realtimePublisher, ILobbyRepository lobbyRepository, ILogger<LobbyHostChangedDomainEventHandler> logger)
    : IDomainEventHandler<LobbyHostChangedDomainEvent>
{
    private readonly ILobbyRepository _lobbyRepository = lobbyRepository;
    private readonly IRealtimePublisher _realtimePublisher = realtimePublisher;
    private readonly ILogger<LobbyHostChangedDomainEventHandler> _logger = logger;

    public async Task<Result<LobbyHostChangedDomainEvent>> HandleAsync(LobbyHostChangedDomainEvent domainEvent, CancellationToken ct = default)
    {
        Lobby? lobby = await _lobbyRepository.GetAsync(domainEvent.LobbyId, ct);

        if (lobby is null) return Result.Failure<LobbyHostChangedDomainEvent>(DomainError.NotFound);

        var tasks = LobbyDto.UserViewsFromLobby(lobby)
            .Select(d => _realtimePublisher.UpdateLobbyAsync(domainEvent.LobbyId, d.Key, DateTimeOffset.Now, nameof(LobbyHostChangedDomainEvent), d.Value, ct));

        Task.WaitAll(tasks, ct);

        return Result<LobbyHostChangedDomainEvent>.Success(domainEvent);
    }
}