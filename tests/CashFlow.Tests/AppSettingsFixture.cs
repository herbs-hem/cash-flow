using Microsoft.Extensions.Configuration;

namespace CashFlow.Tests;

public class AppSettingsFixture
{
    public IConfiguration Configuration { get; }

    public AppSettingsFixture()
    {
        Configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory()) // ou usar AppContext.BaseDirectory
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();
    }
}
