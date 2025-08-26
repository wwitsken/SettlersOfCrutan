using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SettlersOfCrutan.Application.Todos;
using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.Todos;

namespace SettlersOfCrutan.Presentation.Endpoints;


public static class TodoListEndpoints
{
    public static IEndpointRouteBuilder MapTodoListEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/todo-lists")
                       .WithTags("TodoLists");

        // POST /todo-lists
        group.MapPost("/", async Task<Results<Created, ValidationProblem>> (
            [FromBody] CreateTodoListCommand command,
            CreateTodoListCommandHandler handler,
            CancellationToken ct) =>
        {
            var cmd = new CreateTodoListCommand(command.Title!);

            Result<TodoListId> result = await handler.Handle(cmd, ct);

            return result.IsSuccess
            ? TypedResults.Created($"/todo-lists/{result.Value.Value}")
            : TypedResults.ValidationProblem(new Dictionary<string, string[]> { [result.Error.Code] = [result.Error.Message] });
        });

        group.MapGet("/{id:guid}", async Task<Results<Ok<TodoList>, NotFound, ValidationProblem>> (
            Guid id,
            GetTodoListByIdQueryHandler handler,
            CancellationToken ct) =>
                {
                    var query = new GetTodoListByIdQuery(new TodoListId { Value = id });

                    var result = await handler.Handle(query, ct);

                    if (result.IsSuccess && result.Value is not null)
                    {
                        var list = result.Value;
                        return TypedResults.Ok(list);
                    }

                    // If handler uses validation errors (e.g., bad id format), surface them
                    if (result.IsFailure && result.Error.Code == "Validation")
                    {
                        return TypedResults.ValidationProblem(new Dictionary<string, string[]>
                        {
                            [result.Error.Code] = new[] { result.Error.Message }
                        });
                    }

                    return TypedResults.NotFound();
                });

        return app;
    }
}