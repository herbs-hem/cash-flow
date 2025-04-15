using CashFlow.Api.Domain.Aggregates.CashFlow;
using CashFlow.Api.Infrastructure.Persistence.NoSql.Interfaces;
using CashFlow.Api.Infrastructure.Persistence.NoSql.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace CashFlow.Api.Infrastructure.Persistence.NoSql.Repository;

public class MongoSnapshotStore : ISnapshotStore
{
    private readonly IMongoCollection<CashFlowSnapshot> _collection;

    public MongoSnapshotStore(IMongoDatabase db)
    {
        _collection = db.GetCollection<CashFlowSnapshot>("snapshots");
    }

    public async Task<CashFlowSnapshot?> GetLastSnapshotAsync(Guid companyAccountId)
    {
        var filter = Builders<CashFlowSnapshot>.Filter.Eq(s => s.CompanyAccountId, companyAccountId);
        return await _collection.Find(filter).SortByDescending(s => s.Date).FirstOrDefaultAsync();
    }

    public async Task<CashFlowSnapshot?> GetSnapshotAsync(string aggregateId)
    {
        var filter = Builders<CashFlowSnapshot>.Filter.Eq(s => s.AggregateId, aggregateId);
        return await _collection.Find(filter).SortByDescending(s => s.Date).FirstOrDefaultAsync();
    }

    public async Task SaveSnapshotAsync(CashFlowAggregateRoot aggregate)
    {
        var snapshot = new CashFlowSnapshot
        {
            CompanyAccountId = aggregate.CompanyAccountId,
            AggregateId = aggregate.AggregateId,
            Date = aggregate.Date,
            BalanceStart = aggregate.BalanceStartDay,
            BalanceEnd = aggregate.BalanceEndDay,
            AggregateData = aggregate.ToBsonDocument(aggregate.GetType()),
            LastEventId = Guid.NewGuid()
        };

        var filter = Builders<CashFlowSnapshot>.Filter.Eq(s => s.AggregateId, aggregate.AggregateId);
        await _collection.ReplaceOneAsync(filter, snapshot, new ReplaceOptions { IsUpsert = true });
    }
}