using Microsoft.Extensions.Logging;
using SettlersOfCrutan.Application.Abstractions;
using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.DomainErrors;
using SettlersOfCrutan.Domain.Lobbies;
using SettlersOfCrutan.Domain.Lobbies.DomainEvents;

namespace SettlersOfCrutan.Application.Games.DomainEventHandlers;

public sealed class LobbyMemberReadyStatusChangedDomainEventHandler(IRealtimePublisher realtimePublisher, ILobbyRepository lobbyRepository, ILogger<LobbyMemberReadyStatusChangedDomainEventHandler> logger)
    : IDomainEventHandler<LobbyMemberReadyStatusChangedDomainEvent>
{
    private readonly ILobbyRepository _lobbyRepository = lobbyRepository;
    private readonly IRealtimePublisher _realtimePublisher = realtimePublisher;
    private readonly ILogger<LobbyMemberReadyStatusChangedDomainEventHandler> _logger = logger;

    public async Task<Result<LobbyMemberReadyStatusChangedDomainEvent>> HandleAsync(LobbyMemberReadyStatusChangedDomainEvent domainEvent, CancellationToken ct = default)
    {
        Lobby? lobby = await _lobbyRepository.GetAsync(domainEvent.LobbyId, ct);

        if (lobby is null) return Result.Failure<LobbyMemberReadyStatusChangedDomainEvent>(DomainError.NotFound);

        IReadOnlyList<string> recipients = [.. lobby.Members
            .Where(m => m.PlayerId is not null /* && m.PlayerId != domainEvent.UserId */)
            .Select(m => m.PlayerId!.Value.ToString())];

        var message = new LobbyMemberReadyStatusChangedPayload(domainEvent.UserId.ToString(), domainEvent.IsReady);

        await _realtimePublisher.ToLobbyUsersAsync(domainEvent.LobbyId, recipients, nameof(LobbyMemberReadyStatusChangedDomainEvent), message, ct);

        _logger.LogInformation("Player has set themselves as ready or unready successfully!");

        return Result<LobbyMemberReadyStatusChangedDomainEvent>.Success(domainEvent);
    }
}

public sealed record LobbyMemberReadyStatusChangedPayload(string UserId, bool IsReady);