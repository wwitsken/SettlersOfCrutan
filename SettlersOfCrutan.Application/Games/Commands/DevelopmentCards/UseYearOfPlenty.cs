using SettlersOfCrutan.Application.Abstractions;
using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.DomainErrors;
using SettlersOfCrutan.Domain.Games;
using SettlersOfCrutan.Domain.Games.Resources;

namespace SettlersOfCrutan.Application.Games.Commands.DevelopmentCards;

public record UseYearOfPlentyCommandResult(ResourceCardType ResourceType1, ResourceCardType ResourceType2);
public record UseYearOfPlentyCommand(GameId GameId, PlayerId PlayerId, ResourceCardType Resource1, ResourceCardType Resource2) : ICommand<UseYearOfPlentyCommandResult>;

public sealed class UseYearOfPlentyCommandHandler(IGameRepository gameRepository) : ICommandHandler<UseYearOfPlentyCommand, UseYearOfPlentyCommandResult>
{
    private readonly IGameRepository _gameRepository = gameRepository;

    public async Task<Result<UseYearOfPlentyCommandResult>> Handle(UseYearOfPlentyCommand command, CancellationToken ct = default)
    {
        var game = await _gameRepository.GetAsync(command.GameId, ct);
        if (game is null) return Result<UseYearOfPlentyCommandResult>.Failure(DomainError.NotFound);

        var result = game.PlayYearOfPlenty(command.PlayerId, command.Resource1, command.Resource2);
        if (result.IsFailure) return Result<UseYearOfPlentyCommandResult>.Failure(result.Error);

        var saved = await _gameRepository.SaveAsync(game, ct);
        return saved ? Result<UseYearOfPlentyCommandResult>.Success(new(result.Value.t1, result.Value.t2)) :
            Result<UseYearOfPlentyCommandResult>.Failure(DomainError.InvalidOperation);
    }
}
