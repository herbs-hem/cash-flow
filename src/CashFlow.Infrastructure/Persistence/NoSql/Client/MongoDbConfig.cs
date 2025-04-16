using MongoDB.Driver;

namespace CashFlow.Infrastructure.Persistence.NoSql.Client;

// OrderSystem.CashFlow.Infrastructure/Persistence/MongoDbConfig.cs

public static class MongoDbConfig
{
    public static IMongoDatabase ConfigureMongoDb(string connectionString, string databaseName)
    {
        var client = new MongoClient(connectionString);
        return client.GetDatabase(databaseName);
    }
}
