namespace CashFlow.ApiGateway.Extensions;

using Microsoft.Extensions.Hosting;
using Serilog;

public static class LoggingExtensions
{
    public static void UseSerilogLogging(this IHostBuilder hostBuilder)
    {
        hostBuilder.UseSerilog((context, loggerConfig) =>
        {
            loggerConfig
                .ReadFrom.Configuration(context.Configuration)
                .Enrich.FromLogContext()
                .WriteTo.Console();
        });
    }
}