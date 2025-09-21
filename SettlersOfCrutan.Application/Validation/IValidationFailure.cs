namespace SettlersOfCrutan.Application.Validation;
public interface IValidationFailure
{
    IReadOnlyDictionary<string, string[]>? ValidationErrors { get; }
}