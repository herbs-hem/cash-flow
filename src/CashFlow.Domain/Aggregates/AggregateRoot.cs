using CashFlow.Domain.Events;

namespace CashFlow.Domain.Aggregates;

public abstract class AggregateRoot
{
    private readonly List<IDomainEvent> _domainEvents;
    //public List<IDomainEvent> DomainEvents => _domainEvents;

    public string AggregateId { get; set; } = default!;
    public int Version { get; set; } = 0;
    public Guid LastEventId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime LastUpdateAt { get; set; }
    public DateTime Date { get; set; }
    
    protected AggregateRoot()
    {
        _domainEvents = [];
    }

    protected void AddDomainEvent(IDomainEvent @event)
    {
        _domainEvents.Add(@event);
    }

    protected void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    public IList<IDomainEvent> GetUncommittedEvents() => _domainEvents;

    public void Apply(IDomainEvent @event)
    {
        LastEventId = @event.EventId;
        @event.Version++;
        Version++;
    }
}