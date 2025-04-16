using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CashFlow.Infrastructure.Persistence.NoSql.Models
{
    [BsonIgnoreExtraElements]
    public class MongoEvent
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public Guid Id { get; set; } = Guid.NewGuid();

        [BsonRepresentation(BsonType.String)]
        public string AggregateId { get; set; } = null!;

        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime Timestamp { get; set; }

        [BsonElement("EventType")]
        public string EventType { get; set; } = null!;

        [BsonElement("Data")]
        public BsonDocument Data { get; set; } = null!;
    }
}
