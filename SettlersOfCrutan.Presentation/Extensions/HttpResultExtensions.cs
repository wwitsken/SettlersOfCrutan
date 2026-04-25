using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SettlersOfCrutan.Application.Validation;
using SettlersOfCrutan.Domain.Core;

namespace SettlersOfCrutan.Presentation.Extensions;

public static class HttpResultExtensions
{
    // One method, typed: Ok<T>, Created<T>, NoContent, NotFound, ValidationProblem, BadRequest<ProblemDetails>
    public static Results<Ok<T>, NotFound, ValidationProblem, BadRequest<ProblemDetails>
    > ToHttpResult<T>(this Result<T> result)
    {
        if (result.IsSuccess)
            return TypedResults.Ok(result.Value);

        // Validation → 400 with ProblemDetails (validation dictionary)
        if (result is IValidationFailure vf && vf.ValidationErrors is { Count: > 0 } errors)
        {
            var dict = new Dictionary<string, string[]>(errors);
            return TypedResults.ValidationProblem(dict);
        }

        // Map domain error codes
        var error = result.Error;
        return error.Code switch
        {
            var c when c == DomainError.NotFound.Code => TypedResults.NotFound(),
            _ => TypedResults.BadRequest(new ProblemDetails
            {
                Title = "Request failed",
                Detail = error.Message,
            })
        };
    }

    public static Results<
       NoContent, NotFound, ValidationProblem, BadRequest<ProblemDetails>
   > ToHttpResult(this Result<Nothing> result)
    {
        if (result.IsSuccess && result.Value is not null)
            return TypedResults.NoContent();

        // Validation → 400 with ProblemDetails (validation dictionary)
        if (result is IValidationFailure vf && vf.ValidationErrors is { Count: > 0 } errors)
        {
            var dict = new Dictionary<string, string[]>(errors);
            return TypedResults.ValidationProblem(dict);
        }

        // Map domain error codes
        var error = result.Error;
        return error.Code switch
        {
            var c when c == DomainError.NotFound.Code => TypedResults.NotFound(),
            _ => TypedResults.BadRequest(new ProblemDetails
            {
                Title = "Request failed",
                Detail = error.Message,
            })
        };
    }
}