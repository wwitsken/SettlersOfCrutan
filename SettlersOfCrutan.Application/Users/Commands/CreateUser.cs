using SettlersOfCrutan.Application.Abstractions;
using SettlersOfCrutan.Domain.Core;

namespace SettlersOfCrutan.Application.Users.Commands;

/// <summary>Ensures the authenticated principal has a persisted <see cref="Domain.Users.User"/> (same effect as first <see cref="ICurrentUser.UserId"/> call).</summary>
public sealed record CreateUserCommand : ICommand;

public sealed class CreateUserCommandHandler(ICurrentUser currentUser) : ICommandHandler<CreateUserCommand>
{
    private readonly ICurrentUser _currentUser = currentUser;

    public async Task<Result<Nothing>> Handle(CreateUserCommand command, CancellationToken ct = default)
    {
        _ = await _currentUser.UserId();
        return Result<Nothing>.Success();
    }
}
