namespace CashFlow.Api.Domain.Events;

public record OutflowRequestedEvent(Guid BankAccountId, decimal Amount, string description, DateTime OccurredAt) : IDomainEvent
{
    public Guid OrderTransactionId => Guid.NewGuid();
    public Guid EventId { get; set; } = Guid.NewGuid();
    public DateTime OccurredAt { get; set; } = OccurredAt;
    public int Version { get; set; }
}