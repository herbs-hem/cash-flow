using Microsoft.Extensions.Options;
using CashFlow.Api.Infrastructure.Cache.Client;
using CashFlow.Api.Infrastructure.Cache.Interfaces;
using CashFlow.Api.Infrastructure.Settings;
using StackExchange.Redis;

namespace CashFlow.Api.Infrastructure.Cache
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