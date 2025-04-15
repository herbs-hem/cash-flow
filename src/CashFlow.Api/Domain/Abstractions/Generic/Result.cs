namespace CashFlow.Api.Domain.Abstractions.Generic;

public class Result<T> : Result
{
    public T? Value { get; }

    private Result(bool isSuccess, Error error, T? value = default)
        : base(isSuccess, error)
    {
        Value = value;
    }

    public static Result<T> Success(T value) => new(true, Error.None, value);

    public static Result<T> Success() => new(true, Error.None, default!);

    public static Result<T> Failure(Error error) => new(false, error, default);

    public static Result<T> Failure(T value, Error error) => new(false, error, value);

    public static implicit operator Result<T>(Error error) => Result<T>.Failure(error);
}