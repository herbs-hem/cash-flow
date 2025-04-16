using System.Net;

namespace CashFlow.Domain.Abstractions;

public class HttpResult<T>
{
    public bool IsSuccess { get; }
    public T Value { get; }
    public Error Error { get; }
    public int StatusCode { get; }
    
    protected HttpResult(bool isSuccess, T value, Error error, int statusCode)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
        StatusCode = statusCode;
    }
    
    // Métodos de sucesso
    public static HttpResult<T> Ok(T value) => new HttpResult<T>(true, value, null, (int)HttpStatusCode.OK);
    public static HttpResult<T> Created(T value) => new HttpResult<T>(true, value, null, (int)HttpStatusCode.Created);
    public static HttpResult<T> Accepted(T value) => new HttpResult<T>(true, value, null, (int)HttpStatusCode.Accepted);
    public static HttpResult<T> NoContent() => new HttpResult<T>(true, default, null, (int)HttpStatusCode.NoContent);
    
    // Métodos de erro
    public static HttpResult<T> BadRequest(Error error) => new HttpResult<T>(false, default, error, (int)HttpStatusCode.BadRequest);
    public static HttpResult<T> Unauthorized(Error error) => new HttpResult<T>(false, default, error, (int)HttpStatusCode.Unauthorized);
    public static HttpResult<T> Forbidden(Error error) => new HttpResult<T>(false, default, error, (int)HttpStatusCode.Forbidden);
    public static HttpResult<T> NotFound(Error error) => new HttpResult<T>(false, default, error, (int)HttpStatusCode.NotFound);
    public static HttpResult<T> Conflict(Error error) => new HttpResult<T>(false, default, error, (int)HttpStatusCode.Conflict);
    public static HttpResult<T> InternalServerError(Error error) => new HttpResult<T>(false, default, error, (int)HttpStatusCode.InternalServerError);
    
    // Métodos genéricos para códigos personalizados
    public static HttpResult<T> SuccessWithStatusCode(T value, int statusCode) => new HttpResult<T>(true, value, null, statusCode);
    public static HttpResult<T> FailWithStatusCode(Error error, int statusCode) => new HttpResult<T>(false, default, error, statusCode);
}