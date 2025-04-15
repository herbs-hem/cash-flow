namespace CashFlow.Api.Domain.Events;

public record InflowRequestedEvent(Guid BankAccountId, decimal Amount, string Description, DateTime OccurredAt) : IDomainEvent
{
    public Guid OrderTransactionId => Guid.NewGuid();
    public Guid EventId { get; set; } = Guid.NewGuid();
    public DateTime OccurredAt { get; set; } = OccurredAt;
    public int Version { get; set; }
}