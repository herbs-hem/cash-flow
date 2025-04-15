using CashFlow.Api.Infrastructure.Persistence.NoSql.Client;
using CashFlow.Api.Infrastructure.Persistence.NoSql.Interfaces;
using CashFlow.Api.Infrastructure.Persistence.NoSql.Repository;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Bson.Serialization;
using MongoDB.Bson;
using MongoDbSettings = CashFlow.Api.Infrastructure.Settings.MongoDB;

namespace CashFlow.Api.Infrastructure.Persistence.NoSql;

public static class MongoDbExtension
{
    public static IServiceCollection AddNoSqlPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        var mongodbSettings = configuration.GetSection(MongoDbSettings.SectionName).Get<MongoDbSettings>();

        var mongoDatabase = MongoDbConfig.ConfigureMongoDb(mongodbSettings.ConnectionString, mongodbSettings.Database);
        services.AddSingleton(mongoDatabase);

        services.AddScoped<IEventStore, MongoEventStore>();
        services.AddScoped<ISnapshotStore, MongoSnapshotStore>();

        BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));
        BsonSerializer.RegisterSerializer(new DateTimeSerializer(DateTimeKind.Utc, BsonType.String));

        return services;
    }
}