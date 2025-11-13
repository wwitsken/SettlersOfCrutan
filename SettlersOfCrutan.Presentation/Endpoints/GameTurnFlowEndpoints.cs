using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SettlersOfCrutan.Application.Abstractions;
using SettlersOfCrutan.Application.Games.Commands.TurnFlow;
using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.Games;
using SettlersOfCrutan.Domain.Games.Resources;
using SettlersOfCrutan.Presentation.Auth;
using SettlersOfCrutan.Presentation.Dtos;
using SettlersOfCrutan.Presentation.Extensions;

namespace SettlersOfCrutan.Presentation.Endpoints;

public static class GameTurnFlowEndpoints
{
    public static IEndpointRouteBuilder MapGameTurnFlowEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/games/{id:guid}/turn").WithTags("Game:TurnFlow");

        group.MapPost("/end", async Task<Results<Ok<string>, NotFound, ValidationProblem, BadRequest<ProblemDetails>>> (
            Guid id,
            [FromServices] IUserProvider userProvider,
            ICommandHandler<EndTurnCommand, PlayerId> handler,
            CancellationToken ct) =>
        {
            GameId gameId = new() { Value = id };
            PlayerId playerId = PlayerId.Create(userProvider.GetUserId());
            var cmd = new EndTurnCommand(gameId, playerId);
            Result<PlayerId> result = await handler.Handle(cmd, ct);
            return result.UnwrapId<PlayerId, string>().ToHttpResult();
        });

        group.MapPost("/resolve-robber", async Task<Results<Ok<ResourceCardType>, NotFound, ValidationProblem, BadRequest<ProblemDetails>>> (
            Guid id,
            [FromBody] ResolveRobberRequest request,
            IUserProvider userProvider,
            ICommandHandler<ResolveRobberCommand, ResourceCardType> handler,
            CancellationToken ct) =>
        {
            GameId gameId = new() { Value = id };
            PlayerId playerId = PlayerId.Create(userProvider.GetUserId());
            PlayerId victimId = new() { Value = request.VictimPlayerId };
            var cmd = new ResolveRobberCommand(gameId, playerId, request.NewRobberHex.ToDomain(), victimId);
            var result = await handler.Handle(cmd, ct);
            return result.ToHttpResult();
        });

        group.MapPost("/discard-half", async Task<Results<NoContent, NotFound, ValidationProblem, BadRequest<ProblemDetails>>> (
            Guid id,
            [FromBody] DiscardHalfRequest request,
            IUserProvider userProvider,
            ICommandHandler<DiscardHalfCommand> handler,
            CancellationToken ct) =>
        {
            GameId gameId = new() { Value = id };
            PlayerId playerId = PlayerId.Create(userProvider.GetUserId());
            var cmd = new DiscardHalfCommand(gameId, playerId, request.Discards.Select(d => new ResourceCardAmount(d.Type, d.Quantity)).ToList());
            var result = await handler.Handle(cmd, ct);
            return result.ToHttpResult();
        });

        group.RequireAuthorization();

        return app;
    }
}
