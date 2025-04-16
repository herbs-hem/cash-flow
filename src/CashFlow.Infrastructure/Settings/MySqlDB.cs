namespace CashFlow.Infrastructure.Settings;

public class MySqlDB
{
    public const string SectionName = "MySqlDB";
    public string ConnectionString { get; set; }
    public int TimeoutSeconds { get; set; } = 30;
}