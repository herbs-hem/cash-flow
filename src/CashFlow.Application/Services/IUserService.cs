using CashFlow.Application.Dtos;

namespace CashFlow.Application.Services;

public interface IUserService
{
    public Task<Guid> RegisterUserAsync(UserDto dto);
    public Task<UserDto?> ValidateCredentialsAsync(string username, string password);
}