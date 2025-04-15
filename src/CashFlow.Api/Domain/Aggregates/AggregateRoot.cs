using CashFlow.Api.Domain.Events;

namespace CashFlow.Api.Domain.Aggregates;

public abstract class AggregateRoot<TAggregateState>
{
    private readonly IList<IDomainEvent> _domainEvents;

    public string AggregateId { get; set; } = default!;
    public int Version { get; set; } = 0;
    public Guid LastEventId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime LastUpdateAt { get; set; }
    public IList<IDomainEvent> DomainEvents => _domainEvents;

    public abstract TAggregateState Snapshot { get; set; }

    protected AggregateRoot()
    {
        _domainEvents = [];
    }

    protected void AddDomainEvent(IDomainEvent @event)
    {
        _domainEvents.Add(@event);
        LastEventId = @event.EventId;
    }

    protected void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    public abstract void Apply(IDomainEvent @event);
}