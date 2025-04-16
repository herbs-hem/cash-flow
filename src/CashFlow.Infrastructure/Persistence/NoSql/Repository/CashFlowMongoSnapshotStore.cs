using CashFlow.Domain.Aggregates;
using MongoDB.Bson;
using MongoDB.Driver;
using CashFlow.Domain.Aggregates.CashFlow;
using CashFlow.Domain.Documents;
using CashFlow.Infrastructure.Persistence.NoSql.Interfaces;

namespace CashFlow.Infrastructure.Persistence.NoSql.Repository
{
    public class CashFlowMongoSnapshotStore : ISnapshotStore<CashFlowSnapshot, CashFlowAggregateRoot>
    {
        private readonly IMongoCollection<CashFlowSnapshot> _collection;

        public CashFlowMongoSnapshotStore(IMongoDatabase db)
        {
            _collection = db.GetCollection<CashFlowSnapshot>("snapshots");
        }

        public async Task<CashFlowSnapshot?> GetLastSnapshotAsync(Guid companyAccountId)
        {
            var filter = Builders<CashFlowSnapshot>.Filter.Eq(s => s.EntityIdentifier, companyAccountId);
            return await _collection.Find(filter).SortByDescending(s => s.Date).FirstOrDefaultAsync();
        }

        public async Task SaveSnapshotAsync(CashFlowAggregateRoot aggregate)
        {
            var snapshot = new CashFlowSnapshot
            {
                EntityIdentifier = aggregate.CompanyAccountId,
                AggregateId = aggregate.AggregateId,
                Date = aggregate.Date,
                BalanceStart = aggregate.BalanceStartDay,
                BalanceEnd = aggregate.BalanceEndDay,
                AggregateData = aggregate.ToBsonDocument(aggregate.GetType()),
                //AggregateData = JsonConvert.SerializeObject(aggregate),
                LastEventId = Guid.NewGuid()
            };

            var filter = Builders<CashFlowSnapshot>.Filter.Eq(s => s.AggregateId, aggregate.AggregateId);
            await _collection.ReplaceOneAsync(filter, snapshot, new ReplaceOptions { IsUpsert = true });
        }
        
        public async Task<CashFlowSnapshot?> GetSnapshotAsync(string aggregateId)
        {
            var filter = Builders<CashFlowSnapshot>.Filter.Eq(s => s.AggregateId, aggregateId);
            return await _collection.Find(filter).SortByDescending(s => s.Date).FirstOrDefaultAsync();
        }
        
    }
}
