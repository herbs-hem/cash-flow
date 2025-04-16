using AuthService;
using AuthService.Extensions;
using AuthService.Services;
using Microsoft.OpenApi.Models;
using CashFlow.Application.Services;

var builder = WebApplication.CreateBuilder(args);

// 1. Configuração básica do JWT (apenas para geração/validação)
var jwtSettings = builder.Configuration.GetSection("Jwt");
builder.Services.AddSingleton(new JwtSettings(
    jwtSettings["Key"]!,
    jwtSettings["Issuer"]!,
    jwtSettings["Audience"]!
));

// 2. Serviços de autenticação (sem middleware de autenticação)
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAuthService, CashFlow.Application.Services.AuthService>();
builder.Services.AddScoped<ITokenizationService, TokenizationService>();

// 3. Configuração do Swagger (sem segurança)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Token Service API", Version = "v1" });
});

var app = builder.Build();

// 4. Pipeline mínimo
app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();

// 5. Mapeamento de endpoints públicos
app.MapAuthEndpoints(); // Seu método de extensão

app.Run();

// Classe auxiliar para configurações JWT
namespace AuthService
{
    public record JwtSettings(string Key, string Issuer, string Audience);
}