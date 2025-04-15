using MongoDB.Bson.Serialization.Attributes;
using CashFlow.Api.Domain.Events;

namespace CashFlow.Api.Domain.Aggregates.CashFlow;

public class CashFlowAggregateRoot
{
    public string AggregateId { get; private set; }
    [BsonRepresentation(MongoDB.Bson.BsonType.String)]
    public Guid CompanyAccountId { get; init; }
    public decimal BalanceStartDay { get; set; }
    public decimal BalanceEndDay { get; set; }
    public DateTime Date { get; private set; }
    public int Version { get; set; } = 0;

    private List<IDomainEvent> _events = new();

    public CashFlowAggregateRoot(string aggregateId, DateTime date)
    {
        AggregateId = aggregateId;
        CompanyAccountId = Guid.Parse(AggregateId.Split('_')[0]);
        Date = date;
        BalanceStartDay = 0;
        BalanceEndDay = 0;
    }

    public Guid ApplyInflow(decimal amount, string description, DateTime occurredAt)
    {
        var @event = new InflowRequestedEvent(CompanyAccountId, amount, description, DateTime.UtcNow);

        Apply(@event);
        @event.Version++;

        AddEvents(@event);

        return @event.OrderTransactionId;
    }

    public void NotifyInflowProcessed(Guid orderTransactionId, decimal amount, string description, DateTime occurredAt)
    {
        var companyAccountId = Guid.Parse(AggregateId.Split('_')[0]);

        var @event = new InflowProcessedEvent(orderTransactionId, amount, companyAccountId, description, occurredAt, BalanceStartDay, BalanceEndDay);
        @event.Version++;

        AddEvents(@event);
    }

    public Guid ApplyOutflow(decimal amount, string description, DateTime occurredAt)
    {
        IDomainEvent @event;
        Guid orderTransactionId = Guid.Empty;

        if (BalanceEndDay < amount)
            @event = new OutflowFailedEvent(CompanyAccountId, "insufficient Balance", occurredAt);
        else
        {
            @event = new OutflowRequestedEvent(CompanyAccountId, amount, description, occurredAt);
            orderTransactionId = ((OutflowRequestedEvent)@event).OrderTransactionId;
        }

        @event.Version++;

        Apply(@event);
        AddEvents(@event);

        return orderTransactionId;
    }

    public void NotifyOutflowProcessed(Guid orderTransactionId, decimal amount, string description, DateTime occurredAt)
    {
        var companyAccountId = Guid.Parse(AggregateId.Split('_')[0]);

        var @event = new OutflowProcessedEvent(orderTransactionId, amount, companyAccountId, description, occurredAt, BalanceStartDay, BalanceEndDay);
        @event.Version++;

        AddEvents(@event);
    }

    public void Apply(IDomainEvent @event)
    {
        switch (@event)
        {
            case InflowRequestedEvent e:
                BalanceEndDay += e.Amount;
                break;

            case OutflowRequestedEvent e:
                BalanceEndDay -= e.Amount;
                break;
        }
        Version++;
    }

    public IEnumerable<IDomainEvent> GetUncommittedEvents() => _events;

    private void AddEvents(IDomainEvent @event)
    {
        _events ??= new();

        _events.Add(@event);
    }
}