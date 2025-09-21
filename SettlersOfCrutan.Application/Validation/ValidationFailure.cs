using SettlersOfCrutan.Domain.Core;

namespace SettlersOfCrutan.Application.Validation;
public record ValidationFailure<T> : Result<T>, IValidationFailure
{
    public IReadOnlyDictionary<string, string[]>? ValidationErrors { get; }

    private static readonly Error _aggregateValidationError =
        new("Validation.Failed", "One or more validation errors occurred.");

    private ValidationFailure(IReadOnlyDictionary<string, string[]> validationErrors)
        : base(isSuccess: false, value: default, error: _aggregateValidationError)
    {
        ValidationErrors = validationErrors;
    }

    public static ValidationFailure<T> FromErrors(Dictionary<string, string[]> errorDictionary)
    {
        return new ValidationFailure<T>(errorDictionary);
    }
}