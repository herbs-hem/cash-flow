namespace CashFlow.Infrastructure.Persistence.Cache.Interfaces;

public interface ICacheClient : IAsyncCacheClient, IManageCacheClient
{
    bool TryGetValue<T>(string key, out T value);

    void Set<T>(string key, T value, TimeSpan? expiry = null);

    T GetOrCreate<T>(string key, Func<T> factory, TimeSpan? expiry = null);

    void Remove(string key);

    bool Exists(string key);
    
}