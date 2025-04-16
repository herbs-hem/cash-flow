using System.Net.Sockets;
using CashFlow.Application.EventHandlers;
using MassTransit;
using Microsoft.Extensions.Logging;
using CashFlow.Domain.Events;
using CashFlow.Infrastructure.Settings;
using Polly;
namespace CashFlow.Application.Consumer;

public class EventConsumer<T> : IConsumer<T> 
    where T : class, IDomainEvent
{
    private readonly ILogger<EventConsumer<T>> _logger;
    private readonly AsyncPolicy _resiliencePolicy;
    private readonly CashFlow.Infrastructure.Settings.RabbitMQ.QueueSettings _queueConfig;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IEventHandler<T, Guid> _handler;

    public EventConsumer(
        ILogger<EventConsumer<T>> logger,
        IAppSettings appSettings,
        IPublishEndpoint publishEndpoint,
        IEventHandler<T, Guid> handler)
    {
        _logger = logger;
        _publishEndpoint = publishEndpoint;
        _handler = handler;

        if (!appSettings.RabbitMQSettings.Main.Queues.ContainsKey(typeof(T).Name))
            throw new Exception($"RabbitMQ queue for {typeof(T).Name} does not exist");

        _queueConfig = appSettings.RabbitMQSettings.Main.Queues[typeof(T).Name];
        _resiliencePolicy = CreateResiliencePolicy();
    }

    private AsyncPolicy CreateResiliencePolicy()
    {
        var retryPolicy = Policy
            .Handle<RabbitMqConnectionException>()
            .Or<SocketException>()
            .WaitAndRetryAsync(
                _queueConfig.RetryCount,
                retryAttempt => TimeSpan.FromSeconds(Math.Pow(_queueConfig.RetryInterval, retryAttempt)),
                onRetry: (exception, delay, retryCount, context) =>
                {
                    _logger.LogWarning($"Retry {retryCount} after {delay.TotalSeconds}s due to: {exception.Message}");
                });

        var fallbackPolicy = Policy
            .Handle<RabbitMqConnectionException>()
            .Or<SocketException>()
               .FallbackAsync(
                   fallbackAction: async (context, cancellationToken) =>
                   {
                       if (context.TryGetValue("message", out var messageObj) && messageObj is T message)
                           await SendToDlqAsync(message);
                   },
                   onFallbackAsync: async (exception, context) =>
                   {
                       _logger.LogError("All retries failed. Sending to DLQ. Error: {ErrorMessage}", exception.Message);
                       await Task.CompletedTask;
                   });

        var circuitBreakerPolicy = Policy
            .Handle<RabbitMqConnectionException>()
            .Or<SocketException>()
            .CircuitBreakerAsync(
                exceptionsAllowedBeforeBreaking: _queueConfig.CircuitBreaker.TripThreshold,
                durationOfBreak: _queueConfig.CircuitBreaker.DurationOfBreak,
                onBreak: (ex, breakDelay) =>
                {
                    _logger.LogWarning("Circuit broken! Wait {BreakSeconds}s. Error: {ErrorMessage}", breakDelay.TotalSeconds, ex.Message);
                },
                onReset: () => _logger.LogInformation("Circuit reset!"),
                onHalfOpen: () => _logger.LogInformation("Circuit half-open: Testing..."));

        return Policy.WrapAsync(retryPolicy, circuitBreakerPolicy, fallbackPolicy);
    }

    public async Task Consume(ConsumeContext<T> context)
    {
        var policyContext = new Context { ["message"] = context.Message };
            await _resiliencePolicy.ExecuteAsync(async _ =>
            {
                _logger.LogInformation("Consuming event: {EventType}", typeof(T).Name);
                await _handler.HandleAsync(context.Message);
            }, policyContext);
    }

    private async Task SendToDlqAsync(T message)
    {
        try
        {
            _logger.LogWarning("Sending message to DLQ: {DLQQueue}", _queueConfig.DLQ.Queue);
            await _publishEndpoint.Publish(message, context =>
            {
                context.DestinationAddress = new Uri($"exchange:{_queueConfig.DLQ.Exchange}");
                context.Headers.Set("x-reason", "Max retry attempts reached");
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send message to DLQ");
        }
    }
}