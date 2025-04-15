using Dapper;
using CashFlow.Api.Domain.Entities;
using CashFlow.Api.Infrastructure.Persistence.Sql.Interfaces;

namespace CashFlow.Api.Infrastructure.Persistence.Sql.Repository;

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
            @"INSERT INTO AccountBalances (CompanyAccountId, InitialBalance, FinalBalance, Date)
              VALUES (@CompanyAccountId, @InitialBalance, @FinalBalance, @Date)
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
            "SELECT CompanyAccountId, InitialBalance, FinalBalance, Date FROM AccountBalances WHERE CompanyAccountId = @CompanyAccountId",
            new { CompanyAccountId = companyAccountId });
    }

    public async Task<AccountBalance> GetByAsync(Guid companyAccountId, DateTime date)
    {
        using var connection = _connectionFactory.CreateConnection();

        return await connection.QueryFirstOrDefaultAsync<AccountBalance>(
            @"SELECT CompanyAccountId, InitialBalance, FinalBalance, Date
                FROM AccountBalances
              WHERE CompanyAccountId = @CompanyAccountId
                AND Date >= @Date",
            new
            {
                CompanyAccountId = companyAccountId,
                Date = date
            });
    }

    public async Task<bool> ExistsByAsync(Guid CompanyAccountId)
    {
        using var connection = _connectionFactory.CreateConnection();

        return await connection.QueryFirstOrDefaultAsync<bool>(
            "SELECT 1 FROM AccountBalances WHERE CompanyAccountId = @CompanyAccountId",
            new { CompanyAccountId = CompanyAccountId });
    }
}