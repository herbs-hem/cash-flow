using Dapper;
using CashFlow.Domain.Entities;
using CashFlow.Infrastructure.Persistence.Sql.Interfaces;

namespace CashFlow.Infrastructure.Persistence.Sql.Repository;

public class TransactionRepository : ITransactionRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public TransactionRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<Guid> InsertAsync(Transaction transaction)
    {
        var id = Guid.NewGuid();
        using var connection = _connectionFactory.CreateConnection();

        await connection.ExecuteAsync(
            @"INSERT INTO Transactions (Id, EntityIdentifier, Amount, OperationType, Date, Description)
              VALUES (@Id, @EntityIdentifier, @Amount, @OperationType, @Date, @Description)",
            new
            {
                Id = id,
                BankAccountId = transaction.CompanyAccountId,
                transaction.Amount,
                OperationType = (int)transaction.OperationType,
                Date = DateTime.UtcNow,
                transaction.Description
            });

        return id;
    }

    public async Task<Guid> UpdateAsync(Transaction transaction)
    {
        using var connection = _connectionFactory.CreateConnection();

        await connection.ExecuteAsync(
            @"UPDATE Transactions SET 
                Amount = @Amount,
                OperationType = @OperationType,
                Description = @Description
              WHERE Id = @Id",
            new
            {
                TransactionId = transaction.Id,
                transaction.Amount,
                OperationType = (int)transaction.OperationType,
                transaction.Description
            });

        return transaction.Id;
    }

    public async Task<Transaction> GetAsync(Guid id)
    {
        using var connection = _connectionFactory.CreateConnection();

        var result = await connection.QueryFirstOrDefaultAsync(
            @"SELECT Id, EntityIdentifier, Amount, OperationType, Date, Description 
              FROM Transactions 
              WHERE Id = @Id",
            new { Id = id });

        if (result == null) return null;

        return new Transaction(
            result.Id,
            result.CompanyAccountId,
            result.Amount,
            result.OperationType,
            result.Date,
            result.Description);
    }

    public async Task<IEnumerable<Transaction>> GetByAsync(Guid companyAccountId, DateTime initialDate, DateTime endDate)
    {
        using var connection = _connectionFactory.CreateConnection();

        var results = await connection.QueryAsync(
            @"SELECT Id, EntityIdentifier, Amount, OperationType, Date, Description 
              FROM Transactions 
              WHERE EntityIdentifier = @EntityIdentifier 
              AND Date BETWEEN @InitialDate AND @EndDate
              ORDER BY Date DESC",
            new
            {
                CompanyAccountId = companyAccountId,
                InitialDate = initialDate,
                EndDate = endDate
            });

        return results.Select(r => new Transaction(
            r.Id,
            r.CompanyAccountId,
            r.Amount,
            r.OperationType,
            r.Date,
            r.Description));
    }
}