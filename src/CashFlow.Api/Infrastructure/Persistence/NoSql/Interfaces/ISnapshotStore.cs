using CashFlow.Api.Domain.Aggregates.CashFlow;
using CashFlow.Api.Infrastructure.Persistence.NoSql.Models;

namespace CashFlow.Api.Infrastructure.Persistence.NoSql.Interfaces;

public interface ISnapshotStore
{
    Task<CashFlowSnapshot?> GetSnapshotAsync(string aggregateId);

    Task<CashFlowSnapshot?> GetLastSnapshotAsync(Guid companyAccountId);

    Task SaveSnapshotAsync(CashFlowAggregateRoot aggregate);
}