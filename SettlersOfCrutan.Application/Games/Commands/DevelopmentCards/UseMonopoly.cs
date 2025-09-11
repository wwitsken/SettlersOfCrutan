using SettlersOfCrutan.Application.Abstractions;
using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.DomainErrors;
using SettlersOfCrutan.Domain.Games;
using SettlersOfCrutan.Domain.Games.Resources;

namespace SettlersOfCrutan.Application.Games.Commands.DevelopmentCards;

public record UseMonopolyCommand(GameId GameId, PlayerId PlayerId, ResourceCardType ResourceType) : ICommand<int>;

public sealed class UseMonopolyCommandHandler(IGameRepository gameRepository) : ICommandHandler<UseMonopolyCommand, int>
{
    private readonly IGameRepository _gameRepository = gameRepository;

    public async Task<Result<int>> Handle(UseMonopolyCommand command, CancellationToken ct = default)
    {
        var game = await _gameRepository.GetAsync(command.GameId, ct);
        if (game is null) return Result<int>.Failure(DomainError.NotFound);

        var result = game.PlayMonopoly(command.PlayerId, command.ResourceType);
        if (result.IsFailure) return Result<int>.Failure(result.Error);

        var saved = await _gameRepository.SaveAsync(game, ct);
        return saved ? Result<int>.Success(result.Value) : Result<int>.Failure(DomainError.InvalidOperation);
    }
}
