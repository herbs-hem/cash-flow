namespace CashFlow.Api.Domain.Events;

public sealed record OutflowProcessedEvent(Guid TransactionId, decimal Amount, Guid CompanyAccountId, string Description, DateTime OccurredAt, decimal BalanceStartDay, decimal BalanceEndDay) : IDomainEvent
{
    public Guid EventId { get; set; } = Guid.NewGuid();
    public DateTime OccurredAt { get; set; } = OccurredAt;
    public int Version { get; set; }
}