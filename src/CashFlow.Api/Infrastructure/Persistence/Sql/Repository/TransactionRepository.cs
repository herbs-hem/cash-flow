using Dapper;
using CashFlow.Api.Domain.Entities;
using CashFlow.Api.Infrastructure.Persistence.Sql.Interfaces;
using System.Data;
using CashFlow.Api.Domain.Enums;

namespace CashFlow.Api.Infrastructure.Persistence.Sql.Repository;

public class TransactionRepository : ITransactionRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public TransactionRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<Guid> InsertAsync(Transaction transaction)
    {
        var transactionId = Guid.NewGuid();
        using var connection = _connectionFactory.CreateConnection();

        await connection.ExecuteAsync(
            @"INSERT INTO Transactions (TransactionId, CompanyAccountId, Amount, OperationType, Date, Description)
              VALUES (@TransactionId, @CompanyAccountId, @Amount, @OperationType, @Date, @Description)",
            new
            {
                TransactionId = transactionId,
                transaction.CompanyAccountId,
                transaction.Amount,
                OperationType = (int)transaction.OperationType,
                Date = DateTime.UtcNow,
                transaction.Description
            });

        return transactionId;
    }

    public async Task<Guid> UpdateAsync(Transaction transaction)
    {
        using var connection = _connectionFactory.CreateConnection();

        await connection.ExecuteAsync(
            @"UPDATE Transactions SET
                Amount = @Amount,
                OperationType = @OperationType,
                Description = @Description
              WHERE TransactionId = @TransactionId",
            new
            {
                transaction.TransactionId,
                transaction.Amount,
                OperationType = (int)transaction.OperationType,
                transaction.Description
            });

        return transaction.TransactionId;
    }

    public async Task<Transaction> GetAsync(Guid transactionId)
    {
        using var connection = _connectionFactory.CreateConnection();

        var result = await connection.QueryFirstOrDefaultAsync(
            @"SELECT TransactionId, CompanyAccountId, Amount, OperationType, Date, Description
              FROM Transactions
              WHERE TransactionId = @TransactionId",
            new { TransactionId = transactionId });

        if (result == null) return null;

        return new Transaction(
            result.TransactionId,
            result.CompanyAccountId,
            result.Amount,
            result.OperationType,
            result.Date,
            result.Description);
    }

    public async Task<IEnumerable<Transaction>> GetByAsync(Guid companyAccountId, DateTime initialDate, DateTime endDate, OperationType operationType)
    {
        var operations = OperationType.All.Equals(operationType)
            ? [OperationType.Inflow, OperationType.Outflow]
            : new[] { operationType };

        using var connection = _connectionFactory.CreateConnection();

        var results = await connection.QueryAsync(
            @"SELECT TransactionId, CompanyAccountId, Amount, OperationType, Date, Description
              FROM Transactions
              WHERE CompanyAccountId = @CompanyAccountId
              AND Date BETWEEN @InitialDate AND @EndDate
              AND OperationType IN @OperationTypes
              ORDER BY Date DESC",
            new
            {
                CompanyAccountId = companyAccountId,
                InitialDate = initialDate,
                EndDate = endDate,
                OperationTypes = operations
            });

        return results.Select(r => new Transaction(
            r.CompanyAccountId,
            r.Amount,
            (OperationType)r.OperationType,
            r.Description,
            r.Date,
            r.TransactionId));
    }
}