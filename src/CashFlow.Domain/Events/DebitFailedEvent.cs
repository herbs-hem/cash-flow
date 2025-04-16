namespace CashFlow.Domain.Events;

public sealed record DebitFailedEvent(Guid OrderTransactionId, Guid BankAccountId, string Reason, DateTime OccurredAt) : IDomainEvent
{
    public Guid EventId { get; set; } = Guid.NewGuid();
    
    public Guid OrderTransactionId { get; set; } = OrderTransactionId;
    
    public Guid BankAccountId { get; set; } = BankAccountId;
    public DateTime OccurredAt { get; set; } = OccurredAt;
    public int Version { get; set; }
}