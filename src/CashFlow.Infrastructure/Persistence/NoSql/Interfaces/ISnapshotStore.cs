using CashFlow.Domain.Aggregates;
using CashFlow.Domain.Documents;

namespace CashFlow.Infrastructure.Persistence.NoSql.Interfaces
{
    public interface ISnapshotStore<S, A>
        where S: ISnapShot
        where A: AggregateRoot
    {
        Task<S?> GetSnapshotAsync(string aggregateId);
        Task<S?> GetLastSnapshotAsync(Guid companyAccountId);
        Task SaveSnapshotAsync(A aggregate);
    }
}
