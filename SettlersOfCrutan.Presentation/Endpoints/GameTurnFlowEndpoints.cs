using Microsoft.AspNetCore.Mvc;
using SettlersOfCrutan.Application.Abstractions;
using SettlersOfCrutan.Application.Games.Commands.TurnFlow;
using SettlersOfCrutan.Domain.Games;
using SettlersOfCrutan.Domain.Games.Resources;
using SettlersOfCrutan.Presentation.Dtos;
using SettlersOfCrutan.Presentation.Extensions;

namespace SettlersOfCrutan.Presentation.Endpoints;

public static class GameTurnFlowEndpoints
{
    public static IEndpointRouteBuilder MapGameTurnFlowEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/games/{id:guid}/turn").WithTags("Game:TurnFlow");

        group.MapPost("/end", async Task<IResult> (
            Guid id,
            [FromBody] EndTurnRequest request,
            ICommandHandler<EndTurnCommand, PlayerId> handler,
            CancellationToken ct) =>
        {
            GameId gameId = new() { Value = id };
            PlayerId playerId = new() { Value = request.PlayerId };
            var cmd = new EndTurnCommand(gameId, playerId);
            var result = await handler.Handle(cmd, ct);
            return result.ToHttpResult();
        });

        group.MapPost("/resolve-robber", async Task<IResult> (
            Guid id,
            [FromBody] ResolveRobberRequest request,
            ICommandHandler<ResolveRobberCommand, ResourceCardType> handler,
            CancellationToken ct) =>
        {
            GameId gameId = new() { Value = id };
            PlayerId playerId = new() { Value = request.PlayerId };
            PlayerId victimId = new() { Value = request.VictimPlayerId };
            var cmd = new ResolveRobberCommand(gameId, playerId, request.NewRobberHex.ToDomain(), victimId);
            var result = await handler.Handle(cmd, ct);
            return result.ToHttpResult();
        });

        group.MapPost("/discard-half", async Task<IResult> (
            Guid id,
            [FromBody] DiscardHalfRequest request,
            ICommandHandler<DiscardHalfCommand> handler,
            CancellationToken ct) =>
        {
            GameId gameId = new() { Value = id };
            PlayerId playerId = new() { Value = request.PlayerId };
            var cmd = new DiscardHalfCommand(gameId, playerId, request.Discards.Select(d => new ResourceCardAmount(d.Type, d.Quantity)).ToList());
            var result = await handler.Handle(cmd, ct);
            return result.ToHttpResult();
        });

        return app;
    }
}
