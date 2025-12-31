using Microsoft.Extensions.Logging;
using SettlersOfCrutan.Application.Abstractions;
using SettlersOfCrutan.Application.Lobbies.DTOs;
using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.DomainErrors;
using SettlersOfCrutan.Domain.Lobbies;
using SettlersOfCrutan.Domain.Lobbies.DomainEvents;

namespace SettlersOfCrutan.Application.Lobbies.DomainEventHandlers;

public sealed class LobbyMemberAddedDomainEventHandler(IRealtimePublisher realtimePublisher, ILobbyRepository lobbyRepository, ILogger<LobbyMemberAddedDomainEventHandler> logger)
    : IDomainEventHandler<LobbyMemberAddedDomainEvent>
{
    private readonly ILobbyRepository _lobbyRepository = lobbyRepository;
    private readonly IRealtimePublisher _realtimePublisher = realtimePublisher;
    private readonly ILogger<LobbyMemberAddedDomainEventHandler> _logger = logger;

    public async Task<Result<LobbyMemberAddedDomainEvent>> HandleAsync(LobbyMemberAddedDomainEvent domainEvent, CancellationToken ct = default)
    {
        Lobby? lobby = await _lobbyRepository.GetAsync(domainEvent.LobbyId, ct);
        if (lobby is null) return Result.Failure<LobbyMemberAddedDomainEvent>(DomainError.NotFound);

        IReadOnlyList<string> recipients = [.. lobby.Members.Select(m => m.UserId).OfType<string>()];

        await _realtimePublisher.UpdateLobbyAsync(domainEvent.LobbyId, recipients, DateTimeOffset.Now, nameof(LobbyMemberAddedDomainEvent), LobbyDto.FromLobby(lobby), ct);

        return Result<LobbyMemberAddedDomainEvent>.Success(domainEvent);
    }
}

public sealed record LobbyMemberAddedPayload(string UserId);
