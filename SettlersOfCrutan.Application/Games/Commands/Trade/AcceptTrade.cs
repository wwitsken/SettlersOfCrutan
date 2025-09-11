using SettlersOfCrutan.Application.Abstractions;
using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.DomainErrors;
using SettlersOfCrutan.Domain.Games;

namespace SettlersOfCrutan.Application.Games.Commands.Trade;

public record AcceptTradeCommand(GameId GameId, PlayerId PlayerId, TradeOfferId TradeOfferId) : ICommand;

public sealed class AcceptTradeCommandHandler(IGameRepository gameRepository) : ICommandHandler<AcceptTradeCommand>
{
    private readonly IGameRepository _gameRepository = gameRepository;

    public async Task<Result<Nothing>> Handle(AcceptTradeCommand command, CancellationToken ct = default)
    {
        var game = await _gameRepository.GetAsync(command.GameId, ct);
        if (game is null) return Result<Nothing>.Failure(DomainError.NotFound);

        var result = game.AcceptTrade(command.PlayerId, command.TradeOfferId);
        if (result.IsFailure) return Result<Nothing>.Failure(result.Error);

        var saved = await _gameRepository.SaveAsync(game, ct);
        return saved ? Result<Nothing>.Success() : Result<Nothing>.Failure(DomainError.InvalidOperation);
    }
}
