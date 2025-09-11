using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SettlersOfCrutan.Application.Abstractions;
using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.DomainErrors;
using SettlersOfCrutan.Domain.Games;
using SettlersOfCrutan.Domain.Games.Resources;

namespace SettlersOfCrutan.Application.Games.Commands.Trade;

public record MaritimeTrade3to1Command(GameId GameId, PlayerId PlayerId, ResourceCardType DiscardResource, ResourceCardType RequestResource) : ICommand;

public sealed class MaritimeTrade3to1CommandHandler(IGameRepository gameRepository) : ICommandHandler<MaritimeTrade3to1Command>
{
    private readonly IGameRepository _gameRepository = gameRepository;

    public async Task<Result<Nothing>> Handle(MaritimeTrade3to1Command command, CancellationToken ct = default)
    {
        var game = await _gameRepository.GetAsync(command.GameId, ct);
        if (game is null) return Result<Nothing>.Failure(DomainError.NotFound);

        var result = game.Maritime3to1Trade(command.PlayerId, command.DiscardResource, command.RequestResource);
        if (result.IsFailure) return Result<Nothing>.Failure(result.Error);

        var saved = await _gameRepository.SaveAsync(game, ct);
        return saved ? Result<Nothing>.Success() : Result<Nothing>.Failure(DomainError.InvalidOperation);
    }
}
