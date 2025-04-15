using Microsoft.AspNetCore.Mvc;

namespace CashFlow.Api.EntryPoints.Endpoints.Middleware;

public class ExceptionHandlingExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingExceptionMiddleware> _logger;

    public ExceptionHandlingExceptionMiddleware(RequestDelegate next, ILogger<ExceptionHandlingExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred: {Message}", ex.Message);
            var problemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Server Error",
                Type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.6.1"
            };

            context.Response.StatusCode = StatusCodes.Status500InternalServerError;

            await context.Response.WriteAsJsonAsync(problemDetails);
        }
    }
}