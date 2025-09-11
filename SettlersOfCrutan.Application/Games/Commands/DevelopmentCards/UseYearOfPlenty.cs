using SettlersOfCrutan.Application.Abstractions;
using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.DomainErrors;
using SettlersOfCrutan.Domain.Games;
using SettlersOfCrutan.Domain.Games.Resources;

namespace SettlersOfCrutan.Application.Games.Commands.DevelopmentCards;

public record UseYearOfPlentyCommand(GameId GameId, PlayerId PlayerId, ResourceCardType Resource1, ResourceCardType Resource2) : ICommand<(ResourceCardType t1, ResourceCardType t2)>;

public sealed class UseYearOfPlentyCommandHandler(IGameRepository gameRepository) : ICommandHandler<UseYearOfPlentyCommand, (ResourceCardType t1, ResourceCardType t2)>
{
    private readonly IGameRepository _gameRepository = gameRepository;

    public async Task<Result<(ResourceCardType, ResourceCardType)>> Handle(UseYearOfPlentyCommand command, CancellationToken ct = default)
    {
        var game = await _gameRepository.GetAsync(command.GameId, ct);
        if (game is null) return Result<(ResourceCardType, ResourceCardType)>.Failure(DomainError.NotFound);

        var result = game.PlayYearOfPlenty(command.PlayerId, command.Resource1, command.Resource2);
        if (result.IsFailure) return Result<(ResourceCardType, ResourceCardType)>.Failure(result.Error);

        var saved = await _gameRepository.SaveAsync(game, ct);
        return saved ? Result<(ResourceCardType, ResourceCardType)>.Success(result.Value) : Result<(ResourceCardType, ResourceCardType)>.Failure(DomainError.InvalidOperation);
    }
}
