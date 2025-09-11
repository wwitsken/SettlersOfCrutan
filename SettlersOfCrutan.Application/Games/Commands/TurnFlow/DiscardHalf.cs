using SettlersOfCrutan.Application.Abstractions;
using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.DomainErrors;
using SettlersOfCrutan.Domain.Games;
using SettlersOfCrutan.Domain.Games.Resources;

namespace SettlersOfCrutan.Application.Games.Commands.TurnFlow;

public record DiscardHalfCommand(GameId GameId, PlayerId PlayerId, List<ResourceCardAmount> Discards) : ICommand;

public sealed class DiscardHalfCommandHandler(IGameRepository gameRepository) : ICommandHandler<DiscardHalfCommand>
{
    private readonly IGameRepository _gameRepository = gameRepository;

    public async Task<Result<Nothing>> Handle(DiscardHalfCommand command, CancellationToken ct = default)
    {
        var game = await _gameRepository.GetAsync(command.GameId, ct);
        if (game is null) return Result<Nothing>.Failure(DomainError.NotFound);

        var result = game.DiscardHalf(command.PlayerId, command.Discards);
        if (result.IsFailure) return Result<Nothing>.Failure(result.Error);

        var saved = await _gameRepository.SaveAsync(game, ct);
        return saved ? Result<Nothing>.Success() : Result<Nothing>.Failure(DomainError.InvalidOperation);
    }
}
