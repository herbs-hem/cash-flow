using CashFlow.Domain.Events;

namespace CashFlow.Application.Services;

public interface IEventReprocessorService<TEvent> where TEvent : class, IDomainEvent
{
    Task ReprocessAllAsync();
}
