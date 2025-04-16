namespace CashFlow.Domain.Events;

public sealed record CreditFailedEvent(Guid TransactionId, Guid BankAccountId, string Reason, DateTime OccurredAt) : IDomainEvent
{
    public Guid EventId { get; set; } = Guid.NewGuid();
    public DateTime OccurredAt { get; set; } = OccurredAt;
    public int Version { get; set; }
}