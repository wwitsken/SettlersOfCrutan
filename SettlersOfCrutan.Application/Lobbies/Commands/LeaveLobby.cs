using SettlersOfCrutan.Application.Abstractions;
using SettlersOfCrutan.Application.Lobbies.DTOs;
using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.DomainErrors;
using SettlersOfCrutan.Domain.Games;
using SettlersOfCrutan.Domain.Lobbies;

namespace SettlersOfCrutan.Application.Lobbies.Commands;

public record LeaveLobbyCommand(Guid LobbyId, PlayerId PlayerId) : ICommand;
public sealed class LeaveLobbyCommandHandler(ILobbyRepository lobbyRepository, IRealtimePublisher realtimePublisher) : ICommandHandler<LeaveLobbyCommand>
{
    private readonly ILobbyRepository _lobbyRepository = lobbyRepository;
    private readonly IRealtimePublisher _realtimePublisher = realtimePublisher;

    public async Task<Result<Nothing>> Handle(LeaveLobbyCommand command, CancellationToken ct = default)
    {
        var lobby = await _lobbyRepository.GetAsync(new LobbyId() { Value = command.LobbyId }, ct);

        if (lobby is null) return Result<Nothing>.Failure(DomainError.NotFound);

        var res = lobby.RemoveMember(command.PlayerId);

        if (res.IsFailure) return res;

        await _lobbyRepository.SaveAsync(lobby, ct);

        var tasks = LobbyDto
            .UserViewsFromLobby(lobby)
            .Select(d =>
                _realtimePublisher
                .UpdateLobbyAsync(lobby.Id, d.Key, DateTimeOffset.Now, "UserLeftLobby", d.Value, ct));
        Task.WaitAll(tasks, ct);

        return res;
    }
}