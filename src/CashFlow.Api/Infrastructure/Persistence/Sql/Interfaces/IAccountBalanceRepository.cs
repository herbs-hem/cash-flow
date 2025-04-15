using CashFlow.Api.Domain.Entities;

namespace CashFlow.Api.Infrastructure.Persistence.Sql.Interfaces;

public interface IAccountBalanceRepository
{
    Task<decimal> UpsertAsync(AccountBalance accountBalance);

    Task<AccountBalance> GetByAsync(Guid bankAccountId);

    Task<AccountBalance> GetByAsync(Guid bankAccountId, DateTime date);

    Task<bool> ExistsByAsync(Guid bankAccountId);
}