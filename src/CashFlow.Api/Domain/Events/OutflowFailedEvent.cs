namespace CashFlow.Api.Domain.Events;

public sealed record OutflowFailedEvent(Guid CompanyAccountId, string Reason, DateTime OccurredAt) : IDomainEvent
{
    public Guid EventId { get; set; } = Guid.NewGuid();
    public DateTime OccurredAt { get; set; } = OccurredAt;
    public int Version { get; set; }
}