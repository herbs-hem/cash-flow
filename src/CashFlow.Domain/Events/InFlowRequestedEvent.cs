namespace CashFlow.Domain.Events;

public record InFlowRequestedEvent(Guid CompanyAccountId, decimal Amount, string Description, DateTime OccurredAt) : IDomainEvent
{
    public Guid TransactionId => Guid.NewGuid();
    public Guid EventId { get; set; } = Guid.NewGuid();
    public DateTime OccurredAt { get; set; } = OccurredAt;
    public int Version { get; set; }
}
