using CashFlow.Domain.Entities;

namespace CashFlow.Infrastructure.Persistence.Sql.Interfaces;

public interface IUserRepository
{
    Task<Guid> CreateUserAsync(User user, bool withDefaultRole = true);
    Task AddRoleToUserAsync(Guid userId, Guid roleId);
    Task RemoveRoleFromUserAsync(Guid userId, Guid roleId);
    Task<User?> GetByIdWithRolesAsync(Guid userId);
    Task<User?> GetByUsernameWithRolesAsync(string username);
    Task<Role?> GetRoleByNameAsync(string roleName);
}