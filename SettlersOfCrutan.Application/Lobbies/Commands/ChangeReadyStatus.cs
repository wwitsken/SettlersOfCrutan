using SettlersOfCrutan.Application.Abstractions;
using SettlersOfCrutan.Application.Lobbies.DTOs;
using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.DomainErrors;
using SettlersOfCrutan.Domain.Games;
using SettlersOfCrutan.Domain.Lobbies;

namespace SettlersOfCrutan.Application.Lobbies.Commands;
public record ChangeReadyStatusCommand(Guid LobbyId, PlayerId PlayerId, bool IsReady) : ICommand;

public sealed class ChangeReadyStatusCommandHandler(ILobbyRepository lobbyRepository, IRealtimePublisher realtimePublisher) : ICommandHandler<ChangeReadyStatusCommand>
{
    private readonly ILobbyRepository _lobbyRepository = lobbyRepository;
    private readonly IRealtimePublisher _realtimePublisher = realtimePublisher;
    public async Task<Result<Nothing>> Handle(ChangeReadyStatusCommand command, CancellationToken ct = default)
    {
        var lobby = await _lobbyRepository.GetAsync(new LobbyId() { Value = command.LobbyId }, ct);
        if (lobby is null) return Result<Nothing>.Failure(DomainError.NotFound);
        var res = lobby.SetReady(command.PlayerId, command.IsReady);
        if (res.IsFailure) return res;
        await _lobbyRepository.SaveAsync(lobby, ct);

        var tasks = LobbyDto
            .UserViewsFromLobby(lobby)
            .Select(d =>
                _realtimePublisher
                .UpdateLobbyAsync(lobby.Id, d.Key, DateTimeOffset.Now, "UserChangedReadyStatus", d.Value, ct));
        Task.WaitAll(tasks, ct);

        return res;
    }
}
