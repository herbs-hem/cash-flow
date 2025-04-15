namespace CashFlow.Api.Infrastructure.Cache.Interfaces;

public interface IAsyncCacheClient
{
    Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiry = null);

    Task SetAsync<T>(string key, T value, TimeSpan? expiry = null);

    Task RemoveAsync(string key);

    Task<bool> ExistsAsync(string key);
}