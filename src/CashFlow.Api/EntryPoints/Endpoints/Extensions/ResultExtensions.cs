using CashFlow.Api.Domain.Abstractions;

namespace CashFlow.Api.EntryPoints.Endpoints.Extensions;

public static class ResultExtensions
{
    public static IResult ToProblemDetails(this Result result)
    {
        if (result.IsSuccess)
            throw new InvalidOperationException("Can't convert success result to problem");

        return Results.Problem(
                        type: "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                        title: "Bad Request",
                        statusCode: StatusCodes.Status400BadRequest,
                        extensions: new Dictionary<string, object?>
                        {
                            { "errors", new[] { result.Error } }
                        });
    }
}