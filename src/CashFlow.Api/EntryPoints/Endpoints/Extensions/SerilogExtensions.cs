using Serilog;
using Serilog.Exceptions;
using Serilog.Filters;
using System.Diagnostics.CodeAnalysis;

namespace CashFlow.Api.EntryPoints.Endpoints.Extensions;

[ExcludeFromCodeCoverage]
public static class SerilogExtensions
{
    public static IServiceCollection AddSerilog<TSource>(this IServiceCollection services, string applicationName) where TSource : class
    {
        var serviceProvider = services.BuildServiceProvider(validateScopes: true);
        var configuration = serviceProvider.GetRequiredService<IConfiguration>();

        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .Enrich.WithCorrelationId()
            .Enrich.WithExceptionDetails()
            .Enrich.WithProperty("ApplicationName", $"{applicationName}")
            .Filter.ByExcluding(Matching.FromSource("Microsoft.AspNetCore.StaticFiles"))
            .WriteTo.Async(writeTo => writeTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}"))
            .CreateLogger();

        services.AddLogging(config =>
        {
            config.ClearProviders();
            config.AddSerilog(Log.Logger);
        });

        services.AddSingleton(sp => (Microsoft.Extensions.Logging.ILogger)sp.GetService<ILoggerFactory>().CreateLogger<TSource>());

        Serilog.Debugging.SelfLog.Enable(Console.Error);

        return services;
    }
}
