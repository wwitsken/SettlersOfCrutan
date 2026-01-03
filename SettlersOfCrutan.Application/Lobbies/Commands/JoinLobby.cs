
using SettlersOfCrutan.Application.Abstractions;
using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.DomainErrors;
using SettlersOfCrutan.Domain.Games;
using SettlersOfCrutan.Domain.Lobbies;

namespace SettlersOfCrutan.Application.Lobbies.Commands;
public record JoinLobbyCommand(Guid LobbyId, PlayerId PlayerId) : ICommand;
public sealed class JoinLobbyCommandHandler(ILobbyRepository lobbyRepository) : ICommandHandler<JoinLobbyCommand>
{
    private readonly ILobbyRepository _lobbyRepository = lobbyRepository;

    public async Task<Result<Nothing>> Handle(JoinLobbyCommand command, CancellationToken ct = default)
    {
        var lobby = await _lobbyRepository.GetAsync(new LobbyId() { Value = command.LobbyId }, ct);
        if (lobby is null) return Result<Nothing>.Failure(DomainError.NotFound);

        var res = lobby.AddMember(command.PlayerId);

        if (res.IsFailure) return Result<Nothing>.Failure(res.Error);

        await _lobbyRepository.SaveAsync(lobby, ct);
        return Result.Success();
    }
}
