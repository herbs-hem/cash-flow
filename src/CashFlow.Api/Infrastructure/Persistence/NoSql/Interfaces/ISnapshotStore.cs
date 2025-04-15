using CashFlow.Api.Domain.Aggregates.CashFlow;
using CashFlow.Api.Infrastructure.Persistence.NoSql.Models;

namespace CashFlow.Api.Infrastructure.Persistence.NoSql.Interfaces;

public interface ISnapshotStore
{
    Task<BankAccountSnapshot?> GetSnapshotAsync(string aggregateId);

    Task<BankAccountSnapshot?> GetLastSnapshotAsync(Guid bankAccountId);

    Task SaveSnapshotAsync(CashFlowAggregateRoot aggregate);
}