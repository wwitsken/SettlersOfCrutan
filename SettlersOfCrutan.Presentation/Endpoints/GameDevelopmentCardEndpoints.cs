using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SettlersOfCrutan.Application.Abstractions;
using SettlersOfCrutan.Application.Games.Commands.DevelopmentCards;
using SettlersOfCrutan.Domain.Games;
using SettlersOfCrutan.Presentation.Auth;
using SettlersOfCrutan.Presentation.Dtos;
using SettlersOfCrutan.Presentation.Extensions;

namespace SettlersOfCrutan.Presentation.Endpoints;

public static class GameDevelopmentCardEndpoints
{
    public static IEndpointRouteBuilder MapGameDevelopmentCardEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/games/{id:guid}/devcards").WithTags("Game:DevCards");

        group.MapPost("/road-building", async Task<Results<NoContent, NotFound, ValidationProblem, BadRequest<ProblemDetails>>> (

            Guid id,
            [FromBody] UseRoadBuildingRequest request,
            ICommandHandler<UseRoadBuildingCommand> handler,
            IUserProvider userProvider,
            CancellationToken ct) =>
        {
            GameId gameId = new() { Value = id };
            PlayerId playerId = PlayerId.Create(userProvider.GetUserId());
            var cmd = new UseRoadBuildingCommand(gameId, playerId, request.Edge1.ToDomain(), request.Edge2.ToDomain());
            var result = await handler.Handle(cmd, ct);
            return result.ToHttpResult();
        });

        group.MapPost("/monopoly", async Task<Results<Ok<int>, NotFound, ValidationProblem, BadRequest<ProblemDetails>>> (
            Guid id,
            [FromBody] UseMonopolyRequest request,
            IUserProvider userProvider,
            ICommandHandler<UseMonopolyCommand, int> handler,
            CancellationToken ct) =>
        {
            GameId gameId = new() { Value = id };
            PlayerId playerId = PlayerId.Create(userProvider.GetUserId());
            var cmd = new UseMonopolyCommand(gameId, playerId, request.ResourceType);
            var result = await handler.Handle(cmd, ct);
            return result.ToHttpResult();

        });

        group.MapPost("/year-of-plenty", async Task<Results<Ok<UseYearOfPlentyCommandResult>, NotFound, ValidationProblem, BadRequest<ProblemDetails>>> (
            Guid id,
            [FromBody] UseYearOfPlentyRequest request,
            IUserProvider userProvider,
            ICommandHandler<UseYearOfPlentyCommand, UseYearOfPlentyCommandResult> handler,
            CancellationToken ct) =>
        {
            GameId gameId = new() { Value = id };
            PlayerId playerId = PlayerId.Create(userProvider.GetUserId());
            var cmd = new UseYearOfPlentyCommand(gameId, playerId, request.Resource1, request.Resource2);
            var result = await handler.Handle(cmd, ct);
            return result.ToHttpResult();
        });

        group.MapPost("/knight", async Task<Results<NoContent, NotFound, ValidationProblem, BadRequest<ProblemDetails>>> (
            Guid id,
            IUserProvider userProvider,
            ICommandHandler<UseKnightCommand> handler,
            CancellationToken ct) =>
        {
            GameId gameId = new() { Value = id };
            PlayerId playerId = PlayerId.Create(userProvider.GetUserId());
            var cmd = new UseKnightCommand(gameId, playerId);
            var result = await handler.Handle(cmd, ct);
            return result.ToHttpResult();
        });

        group.RequireAuthorization();

        return app;
    }
}
