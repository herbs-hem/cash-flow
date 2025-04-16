namespace CashFlow.Domain.Abstractions;

public class Result<T>
{
    public bool IsSuccess { get; }
    public T Value { get; }
    public Error Error { get; }
    
    private Result(bool isSuccess, T value, Error error)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
    }
    
    public static Result<T> Ok(T value) => new Result<T>(true, value, null);
    public static Result<T> Fail(Error error) => new Result<T>(false, default, error);
}