using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CashFlow.Api.Infrastructure.Persistence.NoSql.Models;

[BsonIgnoreExtraElements]
public class DeadLetterEventDocument
{
    [BsonId]
    [BsonElement("Id")]
    [BsonRepresentation(BsonType.String)]
    public Guid Id { get; set; }

    public string EventType { get; set; } = default!;

    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime CreatedAt { get; set; }

    [BsonElement("EventData")]
    public BsonDocument EventData { get; set; } = default!;
}