using SettlersOfCrutan.Application.Abstractions;
using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.DomainErrors;
using SettlersOfCrutan.Domain.Lobbies;

namespace SettlersOfCrutan.Application.Lobbies.Commands;
public record ChangeReadyStatusCommand(Guid LobbyId, string UserId, bool IsReady) : ICommand;

public sealed class ChangeReadyStatusCommandHandler(ILobbyRepository lobbyRepository) : ICommandHandler<ChangeReadyStatusCommand>
{
    private readonly ILobbyRepository _lobbyRepository = lobbyRepository;
    public async Task<Result<Nothing>> Handle(ChangeReadyStatusCommand command, CancellationToken ct = default)
    {
        var lobby = await _lobbyRepository.GetAsync(new LobbyId() { Value = command.LobbyId }, ct);
        if (lobby is null) return Result<Nothing>.Failure(DomainError.NotFound);
        var res = lobby.SetMemberReady(command.UserId, command.IsReady);
        if (res.IsFailure) return res;
        await _lobbyRepository.SaveAsync(lobby, ct);
        return res;
    }
}
