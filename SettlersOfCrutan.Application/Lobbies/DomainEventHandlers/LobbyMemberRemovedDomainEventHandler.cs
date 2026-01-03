using Microsoft.Extensions.Logging;
using SettlersOfCrutan.Application.Abstractions;
using SettlersOfCrutan.Application.Lobbies.DTOs;
using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.DomainErrors;
using SettlersOfCrutan.Domain.Lobbies;
using SettlersOfCrutan.Domain.Lobbies.DomainEvents;

namespace SettlersOfCrutan.Application.Lobbies.DomainEventHandlers;

public sealed class LobbyMemberRemovedDomainEventHandler(IRealtimePublisher realtimePublisher, ILobbyRepository lobbyRepository, ILogger<LobbyMemberRemovedDomainEventHandler> logger)
    : IDomainEventHandler<LobbyMemberRemovedDomainEvent>
{
    private readonly ILobbyRepository _lobbyRepository = lobbyRepository;
    private readonly IRealtimePublisher _realtimePublisher = realtimePublisher;
    private readonly ILogger<LobbyMemberRemovedDomainEventHandler> _logger = logger;

    public async Task<Result<LobbyMemberRemovedDomainEvent>> HandleAsync(LobbyMemberRemovedDomainEvent domainEvent, CancellationToken ct = default)
    {
        Lobby? lobby = await _lobbyRepository.GetAsync(domainEvent.LobbyId, ct);

        if (lobby is null) return Result.Failure<LobbyMemberRemovedDomainEvent>(DomainError.NotFound);

        var tasks = LobbyDto.UserViewsFromLobby(lobby)
            .Select(d => _realtimePublisher.UpdateLobbyAsync(domainEvent.LobbyId, d.Key, DateTimeOffset.Now, nameof(LobbyMemberRemovedDomainEvent), d.Value, ct));

        Task.WaitAll(tasks, ct);

        return Result<LobbyMemberRemovedDomainEvent>.Success(domainEvent);
    }
}