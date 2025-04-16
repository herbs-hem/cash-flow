using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CashFlow.Domain.Documents;

[BsonIgnoreExtraElements]
public class CashFlowSnapshot: ISnapShot
{
    [BsonId]
    [BsonElement("AggregateId")]
    [BsonRepresentation(BsonType.String)]
    public string AggregateId { get; set; } = default!;

    [BsonElement("Version")]
    [BsonRepresentation(BsonType.Int32)]
    public int Version { get; set; }

    [BsonRepresentation(BsonType.String)]
    public Guid EntityIdentifier { get; set; } = default!;

    [BsonElement("Date")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime Date { get; set; } = default!;

    [BsonRepresentation(BsonType.Decimal128)]
    public decimal BalanceStart { get; set; }

    [BsonRepresentation(BsonType.Decimal128)]
    public decimal BalanceEnd { get; set; }

    [BsonElement("LastEventId")]
    [BsonRepresentation(BsonType.String)]
    public Guid LastEventId { get; set; } = default!;

    [BsonElement("AggregateData")]
    public BsonDocument AggregateData { get; set; } = default!;// JSON do aggregate
}