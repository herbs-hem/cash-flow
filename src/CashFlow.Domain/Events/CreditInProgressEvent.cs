namespace CashFlow.Domain.Events;

public record CreditInProgressEvent(Guid OrderTransactionId, Guid BankAccountId, DateTime OccurredAt) : IDomainEvent
{
    public Guid OrderTransactionId { get; set; } = OrderTransactionId;
    
    public Guid BankAccountId { get; set; } = BankAccountId;
    public Guid EventId { get; set; } = Guid.NewGuid();
    public DateTime OccurredAt { get; set; } = OccurredAt;
    public int Version { get; set; }
}
