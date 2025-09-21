namespace SettlersOfCrutan.Domain.Core;

public record Result<T>
{
    public bool IsSuccess { get; private set; }
    public bool IsFailure => !IsSuccess;

    private readonly T? _value;
    public T Value => !IsSuccess ? throw new InvalidOperationException("Cannot access Value when IsSuccess is false.") : _value!;

    private readonly Error? _error;
    public Error Error => IsSuccess ? throw new InvalidOperationException("Cannot access Error when IsSuccess is true.") : _error!;
    protected Result(bool isSuccess, T? value, Error? error)
    {
        IsSuccess = isSuccess;
        _value = value;
        _error = error;
    }

    public static Result<T> Success(T value) => new(true, value, null);
    public static Result<Nothing> Success() => new(true, new Nothing(), null);
    public static Result<T> Failure(Error error) => new(false, default, error);
}

public static class Result
{
    public static Result<Nothing> Success() => Result<Nothing>.Success();
    public static Result<Nothing> Failure(Error error) => Result<Nothing>.Failure(error);
    public static Result<T> Success<T>(T value) => Result<T>.Success(value);
    public static Result<T> Failure<T>(Error error) => Result<T>.Failure(error);
}
