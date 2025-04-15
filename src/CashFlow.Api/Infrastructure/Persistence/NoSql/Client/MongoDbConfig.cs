using MongoDB.Driver;

namespace CashFlow.Api.Infrastructure.Persistence.NoSql.Client;

public static class MongoDbConfig
{
    public static IMongoDatabase ConfigureMongoDb(string connectionString, string databaseName)
    {
        var client = new MongoClient(connectionString);
        return client.GetDatabase(databaseName);
    }
}