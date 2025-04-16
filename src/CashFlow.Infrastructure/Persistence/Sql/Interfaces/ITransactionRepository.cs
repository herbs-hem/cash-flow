using CashFlow.Domain.Entities;

namespace CashFlow.Infrastructure.Persistence.Sql.Interfaces;

public interface ITransactionRepository
{
    Task<Guid> InsertAsync(Transaction transaction);
    Task<Guid> UpdateAsync(Transaction transaction);
    Task<Transaction> GetAsync(Guid transactionId);
    Task<IEnumerable<Transaction>> GetByAsync(Guid bankAccountId, DateTime initialDate, DateTime endDate);
}