using SettlersOfCrutan.Domain.Core;

namespace SettlersOfCrutan.Application.Validation;
public sealed class ValidationError : Error
{
    private readonly List<ValidationFailure> _errors = new();

    public IReadOnlyList<ValidationFailure> Errors => _errors;
    public bool HasErrors => _errors.Count > 0;

    public ValidationError(
        string code = "validation.failed",
        string message = "One or more validation errors occurred.")
        : base(code, message) { }

    public ValidationError(IEnumerable<ValidationFailure> errors,
        string code = "validation.failed",
        string message = "One or more validation errors occurred.")
        : base(code, message)
    {
        _errors.AddRange(errors);
    }

    public void Add(string field, string message) =>
        _errors.Add(new ValidationFailure(field, message));

    public void Add(ValidationFailure failure) => _errors.Add(failure);

    public void AddRange(IEnumerable<ValidationFailure> failures) =>
        _errors.AddRange(failures);

    public override string ToString() =>
        $"{base.ToString()} ({_errors.Count} validation error(s))";
}