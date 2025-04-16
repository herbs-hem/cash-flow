using CashFlow.Domain.Entities;

namespace CashFlow.Infrastructure.Persistence.Sql.Interfaces;

public interface IAccountBalanceRepository
{
    Task<decimal> UpsertAsync(AccountBalance accountBalance);
    Task<AccountBalance> GetByAsync(Guid bankAccountId);
}
