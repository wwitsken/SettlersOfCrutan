using Microsoft.AspNetCore.Mvc;
using SettlersOfCrutan.Application.Validation;
using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.DomainErrors;

namespace SettlersOfCrutan.Presentation.Extensions;

public static class HttpResultExtensions
{
    public static IResult ToHttpResult<T>(this Result<T> result, bool created = false, string? createdUri = null)
    {
        if (result.IsSuccess)
        {
            if (result.Value is Nothing || result.Value is null)
                return TypedResults.NoContent();

            if (created) return TypedResults.Created(createdUri ?? string.Empty, result.Value);

            return TypedResults.Ok(result.Value);
        }

        // If this is a validation failure, surface the validation dictionary as a ProblemDetails response
        if (result is IValidationFailure vf && vf.ValidationErrors is { } errors && errors.Count > 0)
        {
            // Copy into a concrete dictionary to satisfy the ValidationProblem signature
            var dict = new Dictionary<string, string[]>(errors);
            return TypedResults.ValidationProblem(dict);
        }

        // Fallback to mapping by the single error code/message
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
