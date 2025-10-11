using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SettlersOfCrutan.Application.Abstractions;
using SettlersOfCrutan.Application.Games.Commands.Lifecycle;
using SettlersOfCrutan.Application.Games.Queries;
using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.Games;
using SettlersOfCrutan.Presentation.Auth;
using SettlersOfCrutan.Presentation.Dtos;
using SettlersOfCrutan.Presentation.Extensions;

namespace SettlersOfCrutan.Presentation.Endpoints;

public static class BaseGameEndpoints
{
    public static IEndpointRouteBuilder MapBaseGameEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/games").WithTags("Game:Lifecycle");

        group.MapPost("/create", async Task<Results<Ok<Guid>, NotFound, ValidationProblem, BadRequest<ProblemDetails>>> (
            [FromBody] CreateGameRequest command,
            ICommandHandler<CreateGameCommand, GameId> handler,
            CancellationToken ct) =>
        {
            var cmd = new CreateGameCommand(command.GameName, [.. command.UserIds], command.GameType);
            Result<GameId> result = await handler.Handle(cmd, ct);
            return result.UnwrapId<GameId, Guid>().ToHttpResult();
        });

        group.MapPost("/{id:guid}/join", async Task<Results<Ok<Guid>, NotFound, ValidationProblem, BadRequest<ProblemDetails>>> (
            Guid id,
            [FromBody] JoinGameRequest request,
            IUserProvider userProvider,
            ICommandHandler<JoinGameCommand, GameId> handler,
            CancellationToken ct) =>
        {
            PlayerId playerId = new() { Value = userProvider.GetUserId() };
            GameId gameId = new() { Value = id };
            var cmd = new JoinGameCommand(gameId, playerId);
            var result = await handler.Handle(cmd, ct);
            return result.UnwrapId<GameId, Guid>().ToHttpResult();
        });

        group.MapGet("/{id:guid}", async Task<IResult> (
            Guid id,
            [FromServices] IQueryHandler<GetGameByIdQuery, Game> handler,
            IUserProvider userProvider,
            CancellationToken ct) =>
        {
            var query = new GetGameByIdQuery(new GameId { Value = id });
            var result = await handler.Handle(query, ct);
            return result.ToHttpResult();
        });

        return app;
    }
};