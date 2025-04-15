using CashFlow.Api.Domain.Aggregates.CashFlow;
using CashFlow.Api.Infrastructure.Persistence.NoSql.Interfaces;
using CashFlow.Api.Infrastructure.Persistence.NoSql.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace CashFlow.Api.Infrastructure.Persistence.NoSql.Repository;

public class MongoSnapshotStore : ISnapshotStore
{
    private readonly IMongoCollection<BankAccountSnapshot> _collection;

    public MongoSnapshotStore(IMongoDatabase db)
    {
        _collection = db.GetCollection<BankAccountSnapshot>("snapshots");
    }

    public async Task<BankAccountSnapshot?> GetLastSnapshotAsync(Guid bankAccountId)
    {
        var filter = Builders<BankAccountSnapshot>.Filter.Eq(s => s.BankAccountId, bankAccountId);
        return await _collection.Find(filter).SortByDescending(s => s.Date).FirstOrDefaultAsync();
    }

    public async Task<BankAccountSnapshot?> GetSnapshotAsync(string aggregateId)
    {
        var filter = Builders<BankAccountSnapshot>.Filter.Eq(s => s.AggregateId, aggregateId);
        return await _collection.Find(filter).SortByDescending(s => s.Date).FirstOrDefaultAsync();
    }

    public async Task SaveSnapshotAsync(CashFlowAggregateRoot aggregate)
    {
        var snapshot = new BankAccountSnapshot
        {
            BankAccountId = aggregate.BankAccountId,
            AggregateId = aggregate.AggregateId,
            Date = aggregate.Date,
            BalanceStart = aggregate.BalanceStartDay,
            BalanceEnd = aggregate.BalanceEndDay,
            AggregateData = aggregate.ToBsonDocument(aggregate.GetType()),
            LastEventId = Guid.NewGuid()
        };

        var filter = Builders<BankAccountSnapshot>.Filter.Eq(s => s.AggregateId, aggregate.AggregateId);
        await _collection.ReplaceOneAsync(filter, snapshot, new ReplaceOptions { IsUpsert = true });
    }
}