using System.Security.Claims;
using CashFlow.Application.Dtos;

namespace CashFlow.Application.Services;

public interface IAuthService
{
    string? GenerateToken(UserDto? user);
    ClaimsPrincipal? ValidateToken(string token);
}