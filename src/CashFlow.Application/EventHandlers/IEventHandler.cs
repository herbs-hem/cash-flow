using CashFlow.Domain.Events;

namespace CashFlow.Application.EventHandlers;

public interface IEventHandler<TEvent> where TEvent : IDomainEvent
{
    Task HandleAsync(TEvent @event);
}

public interface IEventHandler<TEvent, TResponse> 
    where TEvent : IDomainEvent
    where TResponse : struct
{
    Task<TResponse> HandleAsync(TEvent @event);
}
