using SettlersOfCrutan.Application.Abstractions;
using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.DomainErrors;
using SettlersOfCrutan.Domain.Games;
using SettlersOfCrutan.Domain.Games.Resources;

namespace SettlersOfCrutan.Application.Games.Commands.Trade;

public record OfferTradeCommand(GameId GameId, PlayerId PlayerId, List<ResourceCardAmount> Requested, List<ResourceCardAmount> Offered) : ICommand;

public sealed class OfferTradeCommandHandler(IGameRepository gameRepository) : ICommandHandler<OfferTradeCommand>
{
    private readonly IGameRepository _gameRepository = gameRepository;

    public async Task<Result<Nothing>> Handle(OfferTradeCommand command, CancellationToken ct = default)
    {
        var game = await _gameRepository.GetAsync(command.GameId, ct);
        if (game is null) return Result<Nothing>.Failure(DomainError.NotFound);

        var result = game.ProposeTrade(command.PlayerId, command.Requested, command.Offered);
        if (result.IsFailure) return Result<Nothing>.Failure(result.Error);

        var saved = await _gameRepository.SaveAsync(game, ct);
        return saved ? Result<Nothing>.Success() : Result<Nothing>.Failure(DomainError.InvalidOperation);
    }
}
