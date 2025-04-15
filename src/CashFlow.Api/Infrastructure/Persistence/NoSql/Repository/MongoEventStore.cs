using MongoDB.Bson;
using MongoDB.Driver;
using CashFlow.Api.Domain.Events;
using CashFlow.Api.Infrastructure.Persistence.NoSql.Interfaces;
using CashFlow.Api.Infrastructure.Persistence.NoSql.Models;

namespace CashFlow.Api.Infrastructure.Persistence.NoSql.Repository;

public class MongoEventStore : IEventStore
{
    private readonly IMongoCollection<MongoEvent> _collection;

    public MongoEventStore(IMongoDatabase db)
    {
        _collection = db.GetCollection<MongoEvent>("events");
    }

    public async Task AppendEventsAsync(string aggregateId, IEnumerable<IDomainEvent> events)
    {
        var docs = events.Select(e => new MongoEvent
        {
            AggregateId = aggregateId,
            Timestamp = e.OccurredAt,
            EventType = e.GetType().Name,
            Data = e.ToBsonDocument(e.GetType())
        }).ToList();

        if (docs.Count > 0)
            await _collection.InsertManyAsync(docs);
    }

    public async Task<IList<MongoEvent>> GetEventsAsync(string aggregateId)
    {
        var filter = Builders<MongoEvent>.Filter.Eq(e => e.AggregateId, aggregateId);
        return await _collection.Find(filter).SortBy(e => e.Timestamp).ToListAsync();
    }
}