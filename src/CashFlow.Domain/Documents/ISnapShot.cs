using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CashFlow.Domain.Documents;

public interface ISnapShot
{ 
    public string AggregateId { get; set; }
    public int Version { get; set; }
    public Guid LastEventId { get; set; }
    public BsonDocument AggregateData { get; set; }
    public DateTime Date { get; set; }
}