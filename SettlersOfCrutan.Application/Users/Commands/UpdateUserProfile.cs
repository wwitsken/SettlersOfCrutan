using FluentValidation;
using SettlersOfCrutan.Application.Abstractions;
using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.Games;

namespace SettlersOfCrutan.Application.Users.Commands;

public sealed record UpdateUserProfileCommand(string? DisplayName, PlayerColor? PreferredColor) : ICommand;

public sealed class UpdateUserProfileCommandValidator : AbstractValidator<UpdateUserProfileCommand>
{
    public UpdateUserProfileCommandValidator()
    {
        RuleFor(x => x.DisplayName)
            .MaximumLength(80)
            .When(x => x.DisplayName is not null);

        RuleFor(x => x.PreferredColor)
            .Must(c => c is null || c != PlayerColor.None)
            .WithMessage("preferredColor cannot be none.");

        RuleFor(x => x)
            .Must(cmd => cmd.DisplayName is not null || cmd.PreferredColor is not null)
            .WithMessage("At least one of displayName or preferredColor must be provided.");
    }
}

public sealed class UpdateUserProfileCommandHandler(
    ICurrentUser currentUser,
    IUserRepository userRepository) : ICommandHandler<UpdateUserProfileCommand>
{
    private readonly ICurrentUser _currentUser = currentUser;
    private readonly IUserRepository _userRepository = userRepository;

    public async Task<Result<Nothing>> Handle(UpdateUserProfileCommand command, CancellationToken ct = default)
    {
        var userId = await _currentUser.UserId();
        var user = await _userRepository.GetAsync(userId, ct);
        if (user is null)
            return Result<Nothing>.Failure(DomainError.NotFound);

        if (command.DisplayName is not null)
            user.UpdateName(command.DisplayName.Trim());

        if (command.PreferredColor is not null)
            user.UpdatePreferredColor(command.PreferredColor.Value);

        var saved = await _userRepository.SaveAsync(user, ct);
        return saved
            ? Result.Success()
            : Result.Failure(new Error("Persistence", "Failed to save user profile."));
    }
}
