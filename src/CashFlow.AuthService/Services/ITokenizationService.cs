using AuthService.Models.Requests;
using AuthService.Models.Responses;

namespace AuthService.Services;

public interface ITokenizationService
{
    public Task<Result<AuthResponse>> GenerateToken(AuthRequest authRequest);
    public Task<Result<UserResponse>> ValidateToken(string token);
}