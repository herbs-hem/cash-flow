using Microsoft.AspNetCore.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Net; using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace CashFlow.ApiGateway;

public class CustomJwtValidatorMiddleware 
{ 
    private readonly RequestDelegate _next;
    private readonly HttpClient _httpClient;
    private readonly string _authServiceUrl = "https://localhost:7003"; // URL do seu AuthService

    public CustomJwtValidatorMiddleware(RequestDelegate next)
    {
        _next = next;
        _httpClient = new HttpClient();
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var endpoint = context.GetEndpoint();
        if (endpoint == null)
        {
            await _next(context);
            return;
        }

        var authAttr = endpoint.Metadata.GetMetadata<AuthorizeAttribute>();
        if (authAttr == null)
        {
            await _next(context);
            return;
        }

        var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
        if (string.IsNullOrEmpty(token))
        {
            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            await context.Response.WriteAsync("Token não fornecido");
            return;
        }

        var response = await _httpClient.PostAsJsonAsync($"{_authServiceUrl}/auth/validateToken", token);

        if (!response.IsSuccessStatusCode)
        {
            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            await context.Response.WriteAsync("Token inválido");
            return;
        }

        var userInfo = await response.Content.ReadFromJsonAsync<UserResponse>();
        if (userInfo == null)
        {
            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            await context.Response.WriteAsync("Erro ao decodificar token");
            return;
        }

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, userInfo.UserName),
            new Claim(ClaimTypes.Email, userInfo.Email)
        };

        foreach (var role in userInfo.Roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var identity = new ClaimsIdentity(claims, "Custom");
        var principal = new ClaimsPrincipal(identity);

        context.User = principal;

        await _next(context);
    }

    private class UserResponse
    {
        public string Email { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new();
    }
}
