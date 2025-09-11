namespace SettlersOfCrutan.Application.Validation;
public sealed record ValidationFailure
{
    public string Field { get; }
    public string Message { get; }

    public ValidationFailure(string field, string message)
    {
        Field = field;
        Message = message;
    }

    public override string ToString() => $"{Field}: {Message}";
}