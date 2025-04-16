using Newtonsoft.Json;

namespace CashFlow.Infrastructure.Settings;

public class RabbitMQ
{
    public const string SectionName = "Broker";
    public MainSettings Main { get; set; }
    public sealed class MainSettings
    {
        public string Host { get; set; }
        public string VirtualHost { get; set; } = "/";
        public string Username { get; set; }
        public string Password { get; set; }
        public Dictionary<string, QueueSettings> Queues { get; set; }
    }
    public sealed class QueueSettings
    {
        public string Name { get; set; }
        public int RetryCount { get; set; }
        public Double RetryInterval { get; set; }
        public DLQSettings DLQ { get; set; }
        public CircuitBreakerSettings CircuitBreaker { get; set; }
    
        [JsonProperty("RetryInterval")]
        private string _retryInterval { get; set; }
    }

    public sealed class DLQSettings
    {
        public bool Durable { get; set; }
        public bool AutoDelete { get; set; }
        public string Queue { get; set; }
        public string Exchange { get; set; }
        public decimal TTL { get; set; }
    }
    public sealed class CircuitBreakerSettings
    {
        public int TripThreshold { get; set; }
        //public int ActiveThreshold { get; set; }
        // public TimeSpan ResetInterval => TimeSpan.Parse(_resetInterval);
        // public TimeSpan TrackingPeriod => TimeSpan.Parse(_trackingPeriod);
    
        // [JsonProperty("ResetInterval")]
        // private string _resetInterval { get; set; }
        //
        // [JsonProperty("TrackingPeriod")]
        // private string _trackingPeriod { get; set; }

        public TimeSpan DurationOfBreak { get; set; }
    }
}