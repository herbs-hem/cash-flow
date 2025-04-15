using CashFlow.Api.Domain.Entities;

namespace CashFlow.Api.Infrastructure.Persistence.Sql.Interfaces;

public interface IAccountBalanceRepository
{
    Task<decimal> UpsertAsync(AccountBalance accountBalance);

    Task<AccountBalance> GetByAsync(Guid companyAccountId);

    Task<AccountBalance> GetByAsync(Guid companyAccountId, DateTime date);

    Task<bool> ExistsByAsync(Guid companyAccountId);
}