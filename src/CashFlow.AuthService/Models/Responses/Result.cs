using System.Net;

namespace AuthService.Models.Responses;

public class Result<T>
{
    public bool IsSuccess { get; }
    public T Value { get; }
    public Error Error { get; }
    public int StatusCode { get; }
    
    protected Result(bool isSuccess, T value, Error error, int statusCode)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
        StatusCode = statusCode;
    }
    
    // Métodos de sucesso
    public static Result<T> Success(T value) => new Result<T>(true, value, null, (int)HttpStatusCode.OK);
    public static Result<T> Created(T value) => new Result<T>(true, value, null, (int)HttpStatusCode.Created);
    public static Result<T> Accepted(T value) => new Result<T>(true, value, null, (int)HttpStatusCode.Accepted);
    public static Result<T> NoContent() => new Result<T>(true, default, null, (int)HttpStatusCode.NoContent);
    
    // Métodos de erro
    public static Result<T> BadRequest(Error error) => new Result<T>(false, default, error, (int)HttpStatusCode.BadRequest);
    public static Result<T> Unauthorized(Error error) => new Result<T>(false, default, error, (int)HttpStatusCode.Unauthorized);
    public static Result<T> Forbidden(Error error) => new Result<T>(false, default, error, (int)HttpStatusCode.Forbidden);
    public static Result<T> NotFound(Error error) => new Result<T>(false, default, error, (int)HttpStatusCode.NotFound);
    public static Result<T> Conflict(Error error) => new Result<T>(false, default, error, (int)HttpStatusCode.Conflict);
    public static Result<T> InternalServerError(Error error) => new Result<T>(false, default, error, (int)HttpStatusCode.InternalServerError);
    
    // Métodos genéricos para códigos personalizados
    public static Result<T> SuccessWithStatusCode(T value, int statusCode) => new Result<T>(true, value, null, statusCode);
    public static Result<T> FailWithStatusCode(Error error, int statusCode) => new Result<T>(false, default, error, statusCode);
}