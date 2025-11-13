
using SettlersOfCrutan.Application.Abstractions;
using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.DomainErrors;
using SettlersOfCrutan.Domain.Games;
using SettlersOfCrutan.Domain.Lobbies;
using SettlersOfCrutan.Domain.PlayerPresence;

namespace SettlersOfCrutan.Application.Lobbies.Commands;
public record JoinLobbyCommand(Guid LobbyId, PlayerId PlayerId) : ICommand;
public sealed class JoinLobbyCommandHandler(ILobbyRepository lobbyRepository, IPlayerPresenceRepository playerPresenceRepository) : ICommandHandler<JoinLobbyCommand>
{
    private readonly ILobbyRepository _lobbyRepository = lobbyRepository;
    private readonly IPlayerPresenceRepository _playerPresenceRepository = playerPresenceRepository;

    public async Task<Result<Nothing>> Handle(JoinLobbyCommand command, CancellationToken ct = default)
    {
        var lobby = await _lobbyRepository.GetAsync(new LobbyId() { Value = command.LobbyId }, ct);
        if (lobby is null) return Result<Nothing>.Failure(DomainError.NotFound);

        PlayerPresence? presence = await _playerPresenceRepository.GetAsync(new PlayerPresenceId() { Value = command.PlayerId.Value }, ct);
        presence ??= PlayerPresence.CreateNew(command.PlayerId, true, DateTime.Now);

        presence.JoinLobby(lobby.Id, DateTime.Now);
        var res = lobby.AddMember(command.PlayerId);

        if (res.IsFailure) return Result<Nothing>.Failure(res.Error);

        // Could be a point of failure - what happens if one saves and the other doesn't?
        await _lobbyRepository.SaveAsync(lobby, ct);
        await _playerPresenceRepository.SaveAsync(presence, ct);

        return Result.Success();
    }
}
