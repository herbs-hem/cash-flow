using CashFlow.Api.Domain.Entities;
using CashFlow.Api.Domain.Enums;

namespace CashFlow.Api.Infrastructure.Persistence.Sql.Interfaces;

public interface ITransactionRepository
{
    Task<Guid> InsertAsync(Transaction transaction);

    Task<Guid> UpdateAsync(Transaction transaction);

    Task<Transaction> GetAsync(Guid transactionId);

    Task<IEnumerable<Transaction>> GetByAsync(Guid companyAccountId, DateTime initialDate, DateTime endDate, OperationType operationType);
}