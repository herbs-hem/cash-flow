using MassTransit;
using CashFlow.Api.Infrastructure.Settings;
using Polly;
using System.Net.Sockets;
using System.Text.Json;

namespace CashFlow.Api.Infrastructure.Messaging
{
    public class Publisher<T> where T : class
    {
        private readonly ILogger<Publisher<T>> _logger;
        private readonly AsyncPolicy _resiliencePolicy;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly Settings.RabbitMQ.QueueSettings _queueConfig;

        public Publisher(ILogger<Publisher<T>> logger, IAppSettings appSettings, IPublishEndpoint publishEndpoint)
        {
            _logger = logger;
            _publishEndpoint = publishEndpoint;

            // Obtém o nome correto da fila com base no tipo da mensagem
            var queueName = GetQueueName<T>(appSettings);

            if (!appSettings.RabbitMQSettings.Main.Queues.TryGetValue(queueName, out _queueConfig!))
            {
                throw new Exception($"RabbitMQ queue configuration for {queueName} does not exist in appsettings.json");
            }

            _resiliencePolicy = CreateResiliencePolicy();
        }

        private string GetQueueName<TMessage>(IAppSettings appSettings)
        {
            // Obtém o nome da fila definido no appsettings.json, comparando com o nome da classe da mensagem
            var queueEntry = appSettings.RabbitMQSettings.Main.Queues
                .FirstOrDefault(q => q.Value.Name.Equals(typeof(TMessage).Name, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrEmpty(queueEntry.Key))
                return queueEntry.Key;

            throw new InvalidOperationException($"No queue found for message type {typeof(TMessage).Name}");
        }

        private AsyncPolicy CreateResiliencePolicy()
        {
            var retryPolicy = Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(
                    _queueConfig.RetryCount,
                    retryAttempt => TimeSpan.FromSeconds(Math.Pow(_queueConfig.RetryInterval, retryAttempt)),
                    onRetry: (exception, delay, retryCount, context) =>
                    {
                        _logger.LogWarning($"Retry {retryCount} after {delay.TotalSeconds}s due to: {exception.Message}");
                    });

            var fallbackPolicy = Policy
                .Handle<Exception>()
                .FallbackAsync(
                    fallbackAction: async (context, cancellationToken) =>
                    {
                        if (context.TryGetValue("message", out var messageObj) && messageObj is T message)
                            await SendToDlqAsync(message);
                    },
                    onFallbackAsync: (exception, context) =>
                    {
                        _logger.LogError("All retries failed. Sending to DLQ. Error: {ErrorMessage}", exception.Message);
                        return Task.CompletedTask;
                    });

            var circuitBreakerPolicy = Policy
                .Handle<RabbitMqConnectionException>()
                .Or<SocketException>()
                .CircuitBreakerAsync(
                    exceptionsAllowedBeforeBreaking: _queueConfig.CircuitBreaker.TripThreshold,
                    durationOfBreak: _queueConfig.CircuitBreaker.DurationOfBreak,
                    onBreak: (ex, breakDelay) =>
                    {
                        _logger.LogWarning("Circuit broken! Will not attempt for {BreakSeconds}s. Error: {ErrorMessage}",
                            breakDelay.TotalSeconds, ex.Message);
                    },
                    onReset: () => _logger.LogInformation("Circuit reset!"),
                    onHalfOpen: () => _logger.LogInformation("Circuit half-open: Testing..."));

            return Policy.WrapAsync(retryPolicy, circuitBreakerPolicy, fallbackPolicy);
        }

        private async Task SendToDlqAsync(T message)
        {
            try
            {
                _logger.LogWarning("Sending message to DLQ: {DLQQueue}", _queueConfig.DLQ.Queue);
                await _publishEndpoint.Publish(message, context =>
                {
                    context.Headers.Set("x-reason", "Max retry attempts reached");
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send message to DLQ");
            }
        }

        public async Task PublishAsync(T message)
        {
            var jsonMessage = JsonSerializer.Serialize(message);

            await _resiliencePolicy.ExecuteAsync(async () =>
            {
                try
                {
                    _logger.LogInformation("Publishing message: {Message}", jsonMessage);
                    await _publishEndpoint.Publish(message);
                }
                catch (Exception ex)

                {
                    _logger.LogError(ex, "Message publishing failed");
                    throw;
                }
            });
        }
    }
}