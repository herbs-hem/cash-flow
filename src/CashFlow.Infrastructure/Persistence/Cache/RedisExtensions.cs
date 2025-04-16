using CashFlow.Infrastructure.Persistence.Cache.Client;
using CashFlow.Infrastructure.Persistence.Cache.Interfaces;
using CashFlow.Infrastructure.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace CashFlow.Infrastructure.Persistence.Cache
{
    public static class RedisExtensions
    {
        public static IServiceCollection AddCachePersistence(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<Redis>(configuration.GetSection(Redis.SectionName));

            services.AddSingleton<IConnectionMultiplexer>(sp =>
            {
                var settings = sp.GetRequiredService<IOptions<Redis>>().Value;
                var config = ConfigurationOptions.Parse($"{settings.Host}");
                config.ConnectTimeout = settings.ConnectTimeout;
                config.ReconnectRetryPolicy = new ExponentialRetry(deltaBackOffMilliseconds: settings.DeltaBackOffMilliseconds);
                config.AbortOnConnectFail = settings.AbortOnConnectFail;

                return ConnectionMultiplexer.Connect(config);
            });

            services
                .AddTransient<ICacheClient, RedisCacheClient>()
                .AddTransient<IAsyncCacheClient, RedisCacheClient>()
                .AddTransient<IManageCacheClient, RedisCacheClient>();

            return services;
        }
    }
}
