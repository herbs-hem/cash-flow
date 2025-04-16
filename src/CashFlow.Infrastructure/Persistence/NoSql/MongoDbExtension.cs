using CashFlow.Domain.Aggregates.CashFlow;
using CashFlow.Domain.Documents;
using CashFlow.Infrastructure.Persistence.NoSql.Client;
using CashFlow.Infrastructure.Persistence.NoSql.Interfaces;
using CashFlow.Infrastructure.Persistence.NoSql.Repository;
using CashFlow.Infrastructure.Settings;
using Microsoft.Extensions.DependencyInjection;

namespace CashFlow.Infrastructure.Persistence.NoSql;

public static class MongoDbExtension
{
    public static IServiceCollection AddNoSqlPersistence(this IServiceCollection services, IAppSettings appSettings)
    {
        var config = appSettings.MongoSettings;

        var mongoDatabase = MongoDbConfig.ConfigureMongoDb(config.ConnectionString, config.Database);
        services.AddSingleton(mongoDatabase);

        services.AddScoped<IEventStore, MongoEventStore>();
        services.AddScoped<ISnapshotStore<CashFlowSnapshot, CashFlowAggregateRoot>, CashFlowMongoSnapshotStore>();

        return services;
    }
}