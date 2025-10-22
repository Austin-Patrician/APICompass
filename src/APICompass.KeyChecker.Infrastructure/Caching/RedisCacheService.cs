using System.Text.Json;
using APICompass.KeyChecker.Core.Interfaces;
using Microsoft.Extensions.Caching.Distributed;

namespace APICompass.KeyChecker.Infrastructure.Caching;

public class RedisCacheService : IValidationCache
{
    private readonly IDistributedCache _cache;
    private readonly TimeSpan _defaultExpiration = TimeSpan.FromMinutes(5);

    public RedisCacheService(IDistributedCache cache)
    {
        _cache = cache;
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class
    {
        var value = await _cache.GetStringAsync(key, cancellationToken);
        
        if (string.IsNullOrEmpty(value))
            return null;

        return JsonSerializer.Deserialize<T>(value);
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default) where T : class
    {
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiration ?? _defaultExpiration
        };

        var json = JsonSerializer.Serialize(value);
        await _cache.SetStringAsync(key, json, options, cancellationToken);
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        await _cache.RemoveAsync(key, cancellationToken);
    }
}
