using CashFlow.Api.Domain.Events;

namespace CashFlow.Api.Infrastructure.Persistence.NoSql.Interfaces;

public interface IEventStore
{
    Task AppendEventsAsync(string aggregateId, IEnumerable<IDomainEvent> events);
}