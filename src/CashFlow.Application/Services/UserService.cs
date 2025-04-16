using CashFlow.Application.Dtos;
using CashFlow.Domain.Entities;
using CashFlow.Infrastructure.Persistence.Sql.Interfaces;

namespace CashFlow.Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Guid> RegisterUserAsync(UserDto dto)
    {
        Guid userId = Guid.NewGuid();
        var user = new User(
            userId,
            dto.Email,
            dto.Username,
            HashPassword(dto.Password),
            DateTime.UtcNow);

       await _userRepository.CreateUserAsync(user);
        
        foreach (var role in dto.Roles)
        {
            Role roleDescription = (await _userRepository.GetRoleByNameAsync(role))!;
            await _userRepository.AddRoleToUserAsync(userId, roleDescription.Id);
        }
        
        return userId;
    }

    public async Task<UserDto?> ValidateCredentialsAsync(string username, string password)
    {
        var user = await _userRepository.GetByUsernameWithRolesAsync(username);
        if (user == null || !VerifyPassword(password, user.Password))
        {
            return null;
        }

        return new UserDto(user.Id, user.Username, user.Email, user.Password, user.Roles.ToList());
    }

    private string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    private bool VerifyPassword(string password, string hashedPassword)
    {
        return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
    }
}