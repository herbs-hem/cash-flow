using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using CashFlow.Api.Domain.Enums;

namespace CashFlow.Api.Domain.Events;

public record RequestedTransactionEvent(Guid TransactionId, Guid BankAccountId, decimal Amount, OperationType OperationType) : IDomainEvent
{
    [BsonRepresentation(BsonType.String)]
    public OperationType OperationType { get; init; } = OperationType;
    public Guid EventId { get; set; }
    public DateTime OccurredAt { get; set; }
    public int Version { get; set; }
}