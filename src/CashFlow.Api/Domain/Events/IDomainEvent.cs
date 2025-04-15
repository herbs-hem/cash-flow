namespace CashFlow.Api.Domain.Events;

public interface IDomainEvent
{
    Guid EventId { get; set; }
    DateTime OccurredAt { get; set; }
    int Version { get; set; }
}