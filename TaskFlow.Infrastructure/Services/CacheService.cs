using Microsoft.Extensions.Caching.Memory;
using TaskFlow.Application.Common.Caching;

namespace TaskFlow.Infrastructure.Services;

/// <summary>
/// Implementación de ICacheService usando IMemoryCache de ASP.NET Core.
/// Caché en memoria del proceso — adecuado para monolitos y entornos single-node.
/// Para múltiples instancias, reemplazar por una implementación Redis (IDistributedCache).
/// </summary>
public sealed class CacheService : ICacheService
{
    private readonly IMemoryCache _cache;

    public CacheService(IMemoryCache cache)
    {
        _cache = cache;
    }

    public T? Get<T>(string key) =>
        _cache.TryGetValue(key, out T? value) ? value : default;

    public void Set<T>(string key, T value, TimeSpan duration) =>
        _cache.Set(key, value, new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = duration
        });

    public void Remove(string key) =>
        _cache.Remove(key);
}
