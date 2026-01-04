using SettlersOfCrutan.Application.Abstractions;
using SettlersOfCrutan.Application.Lobbies.DTOs;
using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.Games;
using SettlersOfCrutan.Domain.Lobbies;
using SettlersOfCrutan.Domain.Lobbies.DomainEvents;

namespace SettlersOfCrutan.Application.Lobbies.Commands;
public record CreateLobbyCommand(PlayerId PlayerId) : ICommand<Guid>;
public sealed class CreateLobbyCommandHandler(ILobbyRepository lobbyRepository) : ICommandHandler<CreateLobbyCommand, Guid>
{
    private readonly ILobbyRepository _lobbyRepository = lobbyRepository;
    public async Task<Result<Guid>> Handle(CreateLobbyCommand command, CancellationToken ct = default)
    {
        var newLobby = Lobby.Create(command.PlayerId);

        await _lobbyRepository.SaveAsync(newLobby, ct);

        return Result.Success(newLobby.Id.Value);
    }
}