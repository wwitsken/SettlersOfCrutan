using SettlersOfCrutan.Application.Abstractions;
using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.Lobbies;

namespace SettlersOfCrutan.Application.Lobbies.Commands;

public record CreateLobbyCommand : ICommand<Guid>;

public sealed class CreateLobbyCommandHandler(ILobbyRepository lobbyRepository, ICurrentUser currentUser) : ICommandHandler<CreateLobbyCommand, Guid>
{
    private readonly ILobbyRepository _lobbyRepository = lobbyRepository;
    private readonly ICurrentUser _currentUser = currentUser;

    public async Task<Result<Guid>> Handle(CreateLobbyCommand command, CancellationToken ct = default)
    {
        var newLobby = Lobby.Create(await _currentUser.UserId());

        await _lobbyRepository.SaveAsync(newLobby, ct);

        return Result.Success(newLobby.Id.Value);
    }
}