using CashFlow.Api.Domain.Events;

namespace CashFlow.Api.Application.Services;

public interface IEventReprocessorService<TEvent> where TEvent : class, IDomainEvent
{
    Task ReprocessAllAsync();
}