using SettlersOfCrutan.Application.Abstractions;
using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.Lobbies;

namespace SettlersOfCrutan.Application.Lobbies.Commands;
public record CreateLobbyCommand(string UserId) : ICommand<Guid>;
public sealed class CreateLobbyCommandHandler(ILobbyRepository lobbyRepository) : ICommandHandler<CreateLobbyCommand, Guid>
{
    private readonly ILobbyRepository _lobbyRepository = lobbyRepository;
    public async Task<Result<Guid>> Handle(CreateLobbyCommand command, CancellationToken ct = default)
    {
        var newLobby = Lobby.Create(command.UserId);

        await _lobbyRepository.SaveAsync(newLobby, ct);

        return Result.Success(newLobby.Id.Value);
    }
}