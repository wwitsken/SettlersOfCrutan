using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SettlersOfCrutan.Application.Games.Commands.Trade;
using SettlersOfCrutan.Domain.Games;
using SettlersOfCrutan.Domain.Games.Resources;
using SettlersOfCrutan.Presentation.Dtos;

namespace SettlersOfCrutan.Presentation.Endpoints;

public static class GameTradeEndpoints
{
    public static IEndpointRouteBuilder MapGameTradeEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/games/{id:guid}/trade").WithTags("Games:Trade");

        group.MapPost("/offer", async Task<Results<Created, ValidationProblem, NotFound>> (
            Guid id,
            [FromBody] OfferTradeRequest request,
            OfferTradeCommandHandler handler,
            CancellationToken ct) =>
        {
            GameId gameId = new() { Value = id };
            PlayerId playerId = new() { Value = request.PlayerId };

            var cmd = new OfferTradeCommand(gameId, playerId, request.Requested.Select(r => new ResourceCardAmount(r.Type, r.Quantity)).ToList(), request.Offered.Select(o => new ResourceCardAmount(o.Type, o.Quantity)).ToList());
            var result = await handler.Handle(cmd, ct);
            if (result.IsSuccess) return TypedResults.Created($"/games/{id}");
            if (result.IsFailure && result.Error.Code == "Validation")
                return TypedResults.ValidationProblem(new Dictionary<string, string[]> { [result.Error.Code] = [result.Error.Message] });
            return TypedResults.NotFound();
        });

        group.MapPost("/accept", async Task<Results<Ok, ValidationProblem, NotFound>> (
            Guid id,
            [FromBody] AcceptTradeRequest request,
            AcceptTradeCommandHandler handler,
            CancellationToken ct) =>
        {
            GameId gameId = new() { Value = id };
            PlayerId playerId = new() { Value = request.PlayerId };
            TradeOfferId tradeOfferId = new() { Value = request.TradeOfferId };

            var cmd = new AcceptTradeCommand(gameId, playerId, tradeOfferId);
            var result = await handler.Handle(cmd, ct);
            if (result.IsSuccess) return TypedResults.Ok();
            if (result.IsFailure && result.Error.Code == "Validation")
                return TypedResults.ValidationProblem(new Dictionary<string, string[]> { [result.Error.Code] = [result.Error.Message] });
            return TypedResults.NotFound();
        });

        group.MapPost("/maritime/4to1", async Task<Results<Ok, ValidationProblem, NotFound>> (
            Guid id,
            [FromBody] MaritimeTradeRequest request,
            MaritimeTrade4to1CommandHandler handler,
            CancellationToken ct) =>
        {
            GameId gameId = new() { Value = id };
            PlayerId playerId = new() { Value = request.PlayerId };
            var cmd = new MaritimeTrade4to1Command(gameId, playerId, request.Discard, request.Request);
            var result = await handler.Handle(cmd, ct);
            if (result.IsSuccess) return TypedResults.Ok();
            if (result.IsFailure && result.Error.Code == "Validation")
                return TypedResults.ValidationProblem(new Dictionary<string, string[]> { [result.Error.Code] = [result.Error.Message] });
            return TypedResults.NotFound();
        });

        group.MapPost("/maritime/3to1", async Task<Results<Ok, ValidationProblem, NotFound>> (
            Guid id,
            [FromBody] MaritimeTradeRequest request,
            MaritimeTrade3to1CommandHandler handler,
            CancellationToken ct) =>
        {
            GameId gameId = new() { Value = id };
            PlayerId playerId = new() { Value = request.PlayerId };
            var cmd = new MaritimeTrade3to1Command(gameId, playerId, request.Discard, request.Request);
            var result = await handler.Handle(cmd, ct);
            if (result.IsSuccess) return TypedResults.Ok();
            if (result.IsFailure && result.Error.Code == "Validation")
                return TypedResults.ValidationProblem(new Dictionary<string, string[]> { [result.Error.Code] = [result.Error.Message] });
            return TypedResults.NotFound();
        });

        group.MapPost("/maritime/2to1", async Task<Results<Ok, ValidationProblem, NotFound>> (
            Guid id,
            [FromBody] MaritimeTradeRequest request,
            MaritimeTrade2to1CommandHandler handler,
            CancellationToken ct) =>
        {
            GameId gameId = new() { Value = id };
            PlayerId playerId = new() { Value = request.PlayerId };
            var cmd = new MaritimeTrade2to1Command(gameId, playerId, request.Discard, request.Request);
            var result = await handler.Handle(cmd, ct);
            if (result.IsSuccess) return TypedResults.Ok();
            if (result.IsFailure && result.Error.Code == "Validation")
                return TypedResults.ValidationProblem(new Dictionary<string, string[]> { [result.Error.Code] = [result.Error.Message] });
            return TypedResults.NotFound();
        });

        return app;
    }
}
