using Dapper;
using CashFlow.Domain.Entities;
using System.Data;
using CashFlow.Infrastructure.Persistence.Sql.Interfaces;

namespace CashFlow.Infrastructure.Persistence.Sql.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public UserRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<Guid> CreateUserAsync(User user, bool withDefaultRole = true)
        {
            using var connection = _connectionFactory.CreateConnection();

            var sql = @"INSERT INTO Users (Id, Email, Username, Password, CreatedAt)
                        VALUES (@Id, @Email, @Username, @Password, @CreatedAt)";
            await connection.ExecuteAsync(sql, user);

            if (withDefaultRole)
            {
                var role = await GetRoleByNameAsync("Customer");
                if (role != null)
                {
                    await AddRoleToUserAsync(user.Id, role.Id);
                }
            }

            return user.Id;
        }

        public async Task AddRoleToUserAsync(Guid userId, Guid roleId)
        {
            using var connection = _connectionFactory.CreateConnection();

            await connection.ExecuteAsync(
                @"INSERT IGNORE INTO UserRoles (UserId, RoleId) VALUES (@UserId, @RoleId)",
                new { UserId = userId, RoleId = roleId });
        }

        public async Task RemoveRoleFromUserAsync(Guid userId, Guid roleId)
        {
            using var connection = _connectionFactory.CreateConnection();

            await connection.ExecuteAsync(
                @"DELETE FROM UserRoles WHERE UserId = @UserId AND RoleId = @RoleId",
                new { UserId = userId, RoleId = roleId });
        }

        public async Task<User?> GetByIdWithRolesAsync(Guid userId)
        {
            using var connection = _connectionFactory.CreateConnection();

            var userDict = new Dictionary<Guid, User>();

            var sql = @"
                SELECT u.Id, u.Email, u.Username, u.Password, u.CreatedAt,
                       r.Id, r.Name
                FROM Users u
                LEFT JOIN UserRoles ur ON u.Id = ur.UserId
                LEFT JOIN Roles r ON ur.RoleId = r.Id
                WHERE u.Id = @UserId";

            var result = await connection.QueryAsync<User, Role, User>(
                sql,
                (user, role) =>
                {
                    if (!userDict.TryGetValue(user.Id, out var existingUser))
                    {
                        existingUser = user;
                        existingUser.UserRoles = new List<UserRole>();
                        userDict[user.Id] = existingUser;
                    }

                    if (role != null)
                    {
                        existingUser.UserRoles.Add(new UserRole(user.Id, role.Id)
                        {
                            Role = role
                        });
                    }

                    return existingUser;
                },
                new { UserId = userId },
                splitOn: "Id");

            return result.FirstOrDefault();
        }

        public async Task<User?> GetByUsernameWithRolesAsync(string username)
        {
            using var connection = _connectionFactory.CreateConnection();

            var userDict = new Dictionary<Guid, User>();

            var sql = @"
                SELECT u.Id, u.Email, u.Username, u.Password, u.CreatedAt,
                       r.Id, r.Name
                FROM Users u
                LEFT JOIN UserRoles ur ON u.Id = ur.UserId
                LEFT JOIN Roles r ON ur.RoleId = r.Id
                WHERE u.Username = @Username";

            var result = await connection.QueryAsync<User, Role, User>(
                sql,
                (user, role) =>
                {
                    if (!userDict.TryGetValue(user.Id, out var existingUser))
                    {
                        existingUser = user;
                        existingUser.UserRoles = new List<UserRole>();
                        userDict[user.Id] = existingUser;
                    }

                    if (role != null)
                    {
                        existingUser.UserRoles.Add(new UserRole(user.Id, role.Id)
                        {
                            Role = role
                        });
                    }

                    return existingUser;
                },
                new { Username = username },
                splitOn: "Id");

            return result.FirstOrDefault();
        }

        public async Task<Role?> GetRoleByNameAsync(string roleName)
        {
            using var connection = _connectionFactory.CreateConnection();

            return await connection.QueryFirstOrDefaultAsync<Role>(
                "SELECT Id, Name FROM Roles WHERE Name = @Name",
                new { Name = roleName });
        }
    }
}