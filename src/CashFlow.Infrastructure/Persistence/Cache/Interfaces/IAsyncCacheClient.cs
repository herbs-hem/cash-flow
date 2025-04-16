namespace CashFlow.Infrastructure.Persistence.Cache.Interfaces;

public interface IAsyncCacheClient
{
    Task<T>? GetAsync<T>(string key);
    
    Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiry = null);

    Task SetAsync<T>(string key, T value, TimeSpan? expiry = null);

    Task RemoveAsync(string key);

    Task<bool> ExistsAsync(string key);
    
    Task<bool> UpsertAsync<T>(string key, T value, TimeSpan? expiry = null);
    
}