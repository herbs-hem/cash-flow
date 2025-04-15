using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CashFlow.Api.Infrastructure.Persistence.NoSql.Models;

[BsonIgnoreExtraElements]
public class EventDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public Guid EventId { get; set; }

    [BsonRepresentation(BsonType.String)]
    public Guid AggregateId { get; set; }

    public string EventType { get; set; } = default!;

    [BsonElement("EventData")]
    public BsonDocument EventData { get; set; } = default!;

    public int Version { get; set; }

    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime OccurredOn { get; set; }
}