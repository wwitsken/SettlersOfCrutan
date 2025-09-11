using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.DomainErrors;

namespace SettlersOfCrutan.Presentation.Extensions;

public static class ErrorHttpResultExtensions
{
    public static IResult ToHttpResult(this Error error)
    {
        return error.Code switch
        {
            var c when c == DomainError.NotFound.Code => TypedResults.NotFound(),
            _ => TypedResults.ValidationProblem(new Dictionary<string, string[]>
            {
                [error.Code] = [error.Message]
            })
        };
    }
}
