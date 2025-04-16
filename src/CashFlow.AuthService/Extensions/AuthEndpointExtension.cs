using System.Net;
using AuthService.Models.Requests;
using AuthService.Models.Responses;
using AuthService.Services;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.Extensions;

public static class AuthEndpointsExtension
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        app.MapPost("/auth/login", 
            async ([FromBody] AuthRequest credentials, ITokenizationService tokenizationService) =>
            {
                var result = await tokenizationService.GenerateToken(credentials);
                return result.ToIResult();
            });
        
        app.MapPost("/auth/validateToken", 
            async ([FromBody] string token, ITokenizationService tokenizationService) =>
            {
                var result = await tokenizationService.ValidateToken(token);
                return result.ToIResult();
            });
    }

    private static IResult ToIResult<T>(this Result<T> result)
    {
        if (result.IsSuccess)
        {
            return result.StatusCode switch
            {
                (int)HttpStatusCode.Created => Results.Created("", result.Value),
                (int)HttpStatusCode.NoContent => Results.NoContent(),
                _ => Results.Ok(result.Value)
            };
        }
        
        return result.StatusCode switch
        {
            (int)HttpStatusCode.BadRequest => Results.BadRequest(result.Error),
            (int)HttpStatusCode.Unauthorized => Results.Unauthorized(),
            (int)HttpStatusCode.NotFound => Results.NotFound(result.Error),
            (int)HttpStatusCode.Conflict => Results.Conflict(result.Error),
            _ => Results.StatusCode(result.StatusCode)
        };
    }
}