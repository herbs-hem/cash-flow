using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace CashFlow.Domain.Documents;

[BsonIgnoreExtraElements]
public class SnapshotDocument
{
    [BsonId]
    [BsonElement("AggregateId")]
    [BsonRepresentation(BsonType.String)]
    public Guid AggregateId { get; set; }

    [BsonElement("Snapshot")]
    public BsonDocument SnapshotData { get; set; } = default!;

    [BsonElement("Version")]
    public int Version { get; set; }

    [BsonElement("DomainEvents")]
    public BsonArray DomainEvents { get; set; } = new BsonArray();

    [BsonElement("LastEventId")]
    [BsonRepresentation(BsonType.String)]
    public Guid LastEventId { get; set; }

    [BsonElement("CreateAt")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime CreateAt { get; set; }

    [BsonElement("LastUpdateAt")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime LastUpdateAt { get; set; }
}
