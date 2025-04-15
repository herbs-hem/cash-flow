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
            @"INSERT INTO AccountBalances (BankAccountId, InitialBalance, FinalBalance, Date)
              VALUES (@BankAccountId, @InitialBalance, @FinalBalance, @Date)
              ON DUPLICATE KEY UPDATE
              InitialBalance = VALUES(InitialBalance),
              FinalBalance = VALUES(FinalBalance),
              Date = VALUES(Date)",
            new
            {
                accountBalance.BankAccountId,
                accountBalance.InitialBalance,
                accountBalance.FinalBalance,
                Date = DateTime.UtcNow
            });

        return accountBalance.FinalBalance;
    }

    public async Task<AccountBalance> GetByAsync(Guid bankAccountId)
    {
        using var connection = _connectionFactory.CreateConnection();

        return await connection.QueryFirstOrDefaultAsync<AccountBalance>(
            "SELECT BankAccountId, InitialBalance, FinalBalance, Date FROM AccountBalances WHERE BankAccountId = @BankAccountId",
            new { BankAccountId = bankAccountId });
    }

    public async Task<AccountBalance> GetByAsync(Guid bankAccountId, DateTime date)
    {
        using var connection = _connectionFactory.CreateConnection();

        return await connection.QueryFirstOrDefaultAsync<AccountBalance>(
            @"SELECT BankAccountId, InitialBalance, FinalBalance, Date
                FROM AccountBalances
              WHERE BankAccountId = @BankAccountId
                AND Date >= @Date",
            new
            {
                BankAccountId = bankAccountId,
                Date = date
            });
    }

    public async Task<bool> ExistsByAsync(Guid bankAccountId)
    {
        using var connection = _connectionFactory.CreateConnection();

        return await connection.QueryFirstOrDefaultAsync<bool>(
            "SELECT 1 FROM AccountBalances WHERE BankAccountId = @BankAccountId",
            new { BankAccountId = bankAccountId });
    }
}