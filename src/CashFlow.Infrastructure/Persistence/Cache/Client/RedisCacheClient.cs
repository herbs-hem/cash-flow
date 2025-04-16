using System.Text.Json;
using CashFlow.Infrastructure.Persistence.Cache.Interfaces;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace CashFlow.Infrastructure.Persistence.Cache.Client;

public class RedisCacheClient : ICacheClient
{
    private readonly ILogger<RedisCacheClient> _logger;
    private readonly IDatabase _database;

    /// <inheritdoc cref="RedisCacheClient" />
    public RedisCacheClient(IConnectionMultiplexer redis, ILogger<RedisCacheClient> logger)
    {
        _logger = logger;
        _database = redis?.GetDatabase() ?? throw new ArgumentNullException(nameof(redis));
    }

    public bool Exists(string key)
    {
        try
        {
            return _database.KeyExists(key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cache existence check failed for key {Key}", key);
            return false;
        }
    }

    public async Task<bool> ExistsAsync(string key)
    {
        try
        {
            return await _database.KeyExistsAsync(key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Async cache existence check failed for key {Key}", key);
            return false;
        }
    }

    public async Task<bool> UpsertAsync<T>(string key, T value, TimeSpan? expiry = null)
    {
        try
        {
            var json = JsonSerializer.Serialize(value);
            return await _database.StringSetAsync(key, json, expiry);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Async cache upsert failed for key {Key}", key);
            return false;
        }
    }

    public T GetOrCreate<T>(string key, Func<T> factory, TimeSpan? expiry = null)
    {
        if (TryGetValue(key, out T value))
        {
            return value;
        }

        value = factory();
        Set(key, value, expiry);
        return value;
    }

    public Task<T>? GetAsync<T>(string key)
    {
        if (TryGetValue(key, out T value))
        {
            return Task.FromResult(value);
        }

        return null;
    }

    public async Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiry = null)
    {
        if (TryGetValue(key, out T value))
        {
            return value;
        }

        value = await factory();
        await SetAsync(key, value, expiry);
        return value;
    }

    public void Remove(string key)
    {
        try
        {
            _database.KeyDelete(key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cache removal failed for key {Key}", key);
        }
    }

    public async Task RemoveAsync(string key)
    {
        try
        {
            await _database.KeyDeleteAsync(key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Async cache removal failed for key {Key}", key);
        }
    }

    public void Set<T>(string key, T value, TimeSpan? expiry = null)
    {
        try
        {
            var json = JsonSerializer.Serialize(value);
            _database.StringSet(key, json, expiry);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cache write failed for key {Key}", key);
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
    {
        try
        {
            var json = JsonSerializer.Serialize(value);
            await _database.StringSetAsync(key, json, expiry);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Async cache write failed for key {Key}", key);
        }
    }

    public bool TryGetValue<T>(string key, out T value)
    {
        try
        {
            var json = _database.StringGet(key);

            if (!json.HasValue)
            {
                value = default!;
                return false;
            }

            value = JsonSerializer.Deserialize<T>(json);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cache read failed for key {Key}", key);
            value = default!;
            return false;
        }
    }

    public TimeSpan? GetTimeToLive(string key)
    {
        try
        {
            return _database.KeyTimeToLive(key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "TTL check failed for key {Key}", key);
            return null;
        }
    }

    public void Refresh(string key, TimeSpan? expiry = null)
    {
        try
        {
            _database.KeyExpire(key, expiry);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cache refresh failed for key {Key}", key);
        }
    }
    
    public Task<IEnumerable<string>> FindKeysByPatternAsync(string pattern)
    {
        try
        {
            var endpoints = _database.Multiplexer.GetEndPoints();
            var server = _database.Multiplexer.GetServer(endpoints.First());
            var keys = server.Keys(pattern: pattern);
            return Task.FromResult(keys.Select(k => k.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to find keys by pattern {Pattern}", pattern);
            return Task.FromResult(Enumerable.Empty<string>());
        }
    }
}