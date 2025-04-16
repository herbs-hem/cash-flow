namespace CashFlow.Infrastructure.Settings;

public class MongoDB
{
    public const string SectionName = "MongoDB";
    public string ConnectionString { get; set; } = default!;
    public string Database { get; set; } = default!;
    public int TimeoutSeconds { get; set; } = 30;
}