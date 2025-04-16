namespace CashFlow.Infrastructure.Settings;

public interface IAppSettings
{
    RabbitMQ RabbitMQSettings { get; }
    MySqlDB DatabaseSettings { get; } 
    MongoDB MongoSettings { get; }
    Redis CacheSettings { get; }
    Task ReloadAsync();
}