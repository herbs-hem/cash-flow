using System.Security.Claims;
using AuthService.Models.Requests;
using AuthService.Models.Responses;
using CashFlow.Application.Services;

namespace AuthService.Services;

public class TokenizationService: ITokenizationService
{
    private IUserService _userService;
    private IAuthService _authService;
    
    public TokenizationService(IUserService userService, IAuthService authService)
    {
        _userService = userService;
        _authService = authService;
    }
    
    public async Task<Result<AuthResponse>> GenerateToken(AuthRequest authRequest)
    {
        var user = _userService.ValidateCredentialsAsync(authRequest.Username, authRequest.Password);
        
        if(user == null)
            return await Task.FromResult(Result<AuthResponse>.BadRequest(new Error("Usuario ou senha inválidos")));
        
        string? token = _authService.GenerateToken(await user);
        
        if (token == null)
            return await Task.FromResult(Result<AuthResponse>.InternalServerError(new Error("Erro ao gerar token")));
        
        return await Task.FromResult(Result<AuthResponse>.Created(new AuthResponse(token)));
    }

    public Task<Result<UserResponse>> ValidateToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return Task.FromResult(Result<UserResponse>.Forbidden(new Error( "Token não fornecido")));

        var principal = _authService.ValidateToken(token);
        if (principal == null) 
            return Task.FromResult(Result<UserResponse>.Forbidden(new Error( "Token inválido ou expirado")));

        var claims = principal.Claims;

        return Task.FromResult(Result<UserResponse>.Accepted(new UserResponse(
            Email: claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value ?? string.Empty,
            UserName: claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value ?? string.Empty,
            Roles: claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToList()
        )));
    }
}