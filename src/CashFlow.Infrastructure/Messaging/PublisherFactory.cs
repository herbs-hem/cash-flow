using CashFlow.Infrastructure.Settings;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace CashFlow.Infrastructure.Messaging;

public class PublisherFactory : IPublisherFactory
{
    private readonly ILogger<PublisherFactory> _logger;
    private readonly IAppSettings _appSettings;
    private readonly IBus _bus;

    public PublisherFactory(
        ILogger<PublisherFactory> logger,
        IAppSettings appSettings,
        IBus bus)
    {
        _logger = logger;
        _appSettings = appSettings;
        _bus = bus;
    }

    public Publisher<T> CreatePublisher<T>() where T : class
    {
        try
        {
            var queueName = typeof(T).Name;
            if (!_appSettings.RabbitMQSettings.Main.Queues.ContainsKey(queueName))
            {
                throw new InvalidOperationException(
                    $"RabbitMQ configuration for message type {queueName} not found. " +
                    $"Please add queue configuration in appsettings.json");
            }

            return new Publisher<T>(
                LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<Publisher<T>>(),
                _appSettings,
                _bus);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create publisher for {MessageType}", typeof(T).Name);
            throw new PublisherCreationException($"Could not create publisher for {typeof(T).Name}", ex);
        }
    }
}

public class PublisherCreationException : Exception
{
    public PublisherCreationException(string message, Exception innerException) 
        : base(message, innerException) { }
}