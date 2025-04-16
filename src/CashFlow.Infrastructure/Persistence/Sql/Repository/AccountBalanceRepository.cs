using Dapper;
using CashFlow.Domain.Entities;
using CashFlow.Infrastructure.Persistence.Sql.Interfaces;

namespace CashFlow.Infrastructure.Persistence.Sql.Repository;

public class AccountBalanceRepository : IAccountBalanceRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public AccountBalanceRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<decimal> UpsertAsync(AccountBalance accountBalance)
    {
        using var connection = _connectionFactory.CreateConnection();

        await connection.ExecuteAsync(
            @"INSERT INTO AccountBalances (EntityIdentifier, InitialBalance, FinalBalance, Date) 
              VALUES (@EntityIdentifier, @InitialBalance, @FinalBalance, @Date)
              ON DUPLICATE KEY UPDATE
              InitialBalance = VALUES(InitialBalance),
              FinalBalance = VALUES(FinalBalance),
              Date = VALUES(Date)",
            new
            {
                accountBalance.CompanyAccountId,
                accountBalance.InitialBalance,
                accountBalance.FinalBalance,
                Date = DateTime.UtcNow
            });

        return accountBalance.FinalBalance;
    }

    public async Task<AccountBalance> GetByAsync(Guid companyAccountId)
    {
        using var connection = _connectionFactory.CreateConnection();

        return await connection.QueryFirstOrDefaultAsync<AccountBalance>(
            "SELECT EntityIdentifier, InitialBalance, FinalBalance, Date FROM AccountBalances WHERE EntityIdentifier = @EntityIdentifier",
            new { CompanyAccountId = companyAccountId });
    }
}