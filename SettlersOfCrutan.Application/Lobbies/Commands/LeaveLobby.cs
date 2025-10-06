using SettlersOfCrutan.Application.Abstractions;
using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.DomainErrors;
using SettlersOfCrutan.Domain.Lobbies;

namespace SettlersOfCrutan.Application.Lobbies.Commands;

public record LeaveLobbyCommand(Guid LobbyId, string UserId) : ICommand;
public sealed class LeaveLobbyCommandHandler(ILobbyRepository lobbyRepository) : ICommandHandler<LeaveLobbyCommand>
{
    private readonly ILobbyRepository _lobbyRepository = lobbyRepository;


    public async Task<Result<Nothing>> Handle(LeaveLobbyCommand command, CancellationToken ct = default)
    {
        var lobby = await _lobbyRepository.GetAsync(new LobbyId() { Value = command.LobbyId }, ct);

        if (lobby is null) return Result<Nothing>.Failure(DomainError.NotFound);

        var res = lobby.RemoveMember(command.UserId);

        if (res.IsFailure) return res;

        await _lobbyRepository.SaveAsync(lobby, ct);

        return res;
    }
}