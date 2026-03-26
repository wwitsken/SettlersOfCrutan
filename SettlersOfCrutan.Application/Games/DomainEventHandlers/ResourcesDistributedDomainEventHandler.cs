using Microsoft.Extensions.Logging;
using SettlersOfCrutan.Application.Abstractions;
using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.DomainErrors;
using SettlersOfCrutan.Domain.Games.DomainEvents;
using SettlersOfCrutan.Domain.Games.Resources;

namespace SettlersOfCrutan.Application.Games.DomainEventHandlers;

public sealed class ResourcesDistributedDomainEventHandler(IRealtimePublisher realtimePublisher, IGameRepository gameRepository, ILogger<ResourcesDistributedDomainEventHandler> logger)
    : IDomainEventHandler<ResourcesDistributedDomainEvent>
{
    private readonly IGameRepository _gameRepository = gameRepository;
    private readonly IRealtimePublisher _realtimePublisher = realtimePublisher;
    private readonly ILogger<ResourcesDistributedDomainEventHandler> _logger = logger;

    public async Task<Result<ResourcesDistributedDomainEvent>> HandleAsync(ResourcesDistributedDomainEvent domainEvent, CancellationToken ct = default)
    {
        var game = await _gameRepository.GetAsync(domainEvent.GameId, ct);
        if (game is null) return Result.Failure<ResourcesDistributedDomainEvent>(DomainError.NotFound);

        IReadOnlyList<string> allRecipients = [.. game.Players.Select(p => p.Id.ToString())];

        // Send each player their own private resource distribution
        foreach (var (playerId, resources) in domainEvent.ResourceDistribution)
        {
            var playerDistribution = resources
                .GroupBy(r => r.Type)
                .Select(g => new ResourceCardAmountDto(g.Key.ToString(), g.Sum(r => r.Quantity)))
                .ToList();

            var privateMessage = new PlayerResourcesReceivedMessage(
                domainEvent.GameId.Value,
                playerId.Value,
                playerDistribution);

            await _realtimePublisher.ToGameUserAsync(domainEvent.GameId, playerId.ToString(), nameof(ResourcesDistributedDomainEvent), privateMessage, ct);
        }

        // Notify all players about the overall distribution (resource types are public since they follow dice + settlements)
        var publicDistribution = domainEvent.ResourceDistribution
            .ToDictionary(
                kvp => kvp.Key.Value,
                kvp => kvp.Value.GroupBy(r => r.Type).Select(g => new ResourceCardAmountDto(g.Key.ToString(), g.Sum(r => r.Quantity))).ToList());

        var publicMessage = new ResourcesDistributedMessage(domainEvent.GameId.Value, publicDistribution);
        await _realtimePublisher.ToGameUsersAsync(domainEvent.GameId, allRecipients, nameof(ResourcesDistributedDomainEvent) + "Public", publicMessage, ct);

        _logger.LogInformation("Game {GameId}: resources distributed to {PlayerCount} players",
            domainEvent.GameId.Value, domainEvent.ResourceDistribution.Count);

        return Result.Success(domainEvent);
    }
}

public record ResourceCardAmountDto(string ResourceType, int Quantity);
public record PlayerResourcesReceivedMessage(Guid GameId, string PlayerId, List<ResourceCardAmountDto> Resources);
public record ResourcesDistributedMessage(Guid GameId, Dictionary<string, List<ResourceCardAmountDto>> Distribution);
