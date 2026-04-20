using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SettlersOfCrutan.Application.Abstractions;
using SettlersOfCrutan.Application.Games.Commands.Trade;
using SettlersOfCrutan.Domain.Games;
using SettlersOfCrutan.Domain.Games.Resources;
using SettlersOfCrutan.Presentation.Dtos;
using SettlersOfCrutan.Presentation.Extensions;

namespace SettlersOfCrutan.Presentation.Endpoints;

public static class GameTradeEndpoints
{
    public static IEndpointRouteBuilder MapGameTradeEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/games/{id:guid}/trade").WithTags("Game:Trade");

        group.MapPost("/offer", async Task<Results<NoContent, NotFound, ValidationProblem, BadRequest<ProblemDetails>>> (
            Guid id,
            [FromBody] OfferTradeRequest request,
            ICommandHandler<OfferTradeCommand> handler,
            CancellationToken ct) =>
        {
            GameId gameId = new() { Value = id };
            var cmd = new OfferTradeCommand(gameId, request.Requested.Select(r => new ResourceCardAmount(r.Type, r.Quantity)).ToList(), request.Offered.Select(o => new ResourceCardAmount(o.Type, o.Quantity)).ToList());
            var result = await handler.Handle(cmd, ct);
            return result.ToHttpResult();
        });

        group.MapPost("/accept", async Task<Results<NoContent, NotFound, ValidationProblem, BadRequest<ProblemDetails>>> (
            Guid id,
            [FromBody] AcceptTradeRequest request,
            ICommandHandler<AcceptTradeCommand> handler,
            CancellationToken ct) =>
        {
            GameId gameId = new() { Value = id };
            TradeOfferId tradeOfferId = new() { Value = request.TradeOfferId };
            var cmd = new AcceptTradeCommand(gameId, tradeOfferId);
            var result = await handler.Handle(cmd, ct);
            return result.ToHttpResult();
        });

        group.MapPost("/maritime/4to1", async Task<Results<NoContent, NotFound, ValidationProblem, BadRequest<ProblemDetails>>> (
            Guid id,
            [FromBody] MaritimeTradeRequest request,
            ICommandHandler<MaritimeTrade4to1Command> handler,
            CancellationToken ct) =>
        {
            GameId gameId = new() { Value = id };
            var cmd = new MaritimeTrade4to1Command(gameId, request.Discard, request.Request);
            var result = await handler.Handle(cmd, ct);
            return result.ToHttpResult();
        });

        group.MapPost("/maritime/3to1", async Task<Results<NoContent, NotFound, ValidationProblem, BadRequest<ProblemDetails>>> (
            Guid id,
            [FromBody] MaritimeTradeRequest request,
            ICommandHandler<MaritimeTrade3to1Command> handler,
            CancellationToken ct) =>
        {
            GameId gameId = new() { Value = id };
            var cmd = new MaritimeTrade3to1Command(gameId, request.Discard, request.Request);
            var result = await handler.Handle(cmd, ct);
            return result.ToHttpResult();
        });

        group.MapPost("/maritime/2to1", async Task<Results<NoContent, NotFound, ValidationProblem, BadRequest<ProblemDetails>>> (
            Guid id,
            [FromBody] MaritimeTradeRequest request,
            ICommandHandler<MaritimeTrade2to1Command> handler,
            CancellationToken ct) =>
        {
            GameId gameId = new() { Value = id };
            var cmd = new MaritimeTrade2to1Command(gameId, request.Discard, request.Request);
            var result = await handler.Handle(cmd, ct);
            return result.ToHttpResult();
        });

        group.RequireAuthorization();

        return app;
    }
}
