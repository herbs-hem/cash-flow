namespace CashFlow.Infrastructure.Settings;

public class Redis
{
    public const string SectionName = "Cache";

    public int ExpirationMinutes { get; set; } = 15;
    public string RedisConnection { get; set; } = default!;

    public string Host { get; set; }
    public int DeltaBackOffMilliseconds { get; set; } = 1000;
    public int ConnectTimeout { get; set; } = 5000;
    public bool AbortOnConnectFail { get; set; } = false;
}