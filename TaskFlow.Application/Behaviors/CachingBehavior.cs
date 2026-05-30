using MediatR;
using Microsoft.Extensions.Logging;
using TaskFlow.Application.Common.Caching;

namespace TaskFlow.Application.Behaviors;

/// <summary>
/// Pipeline de MediatR que intercepta queries que implementan ICacheableQuery.
///
/// Orden de ejecución: LoggingBehavior → CachingBehavior → ValidationBehavior → Handler
///
/// En caso de cache hit, la ejecución cortocircuita aquí:
/// ni la validación ni el handler se ejecutan, evitando cualquier query a BD.
/// </summary>
public sealed class CachingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ICacheService _cache;
    private readonly ILogger<CachingBehavior<TRequest, TResponse>> _logger;

    public CachingBehavior(ICacheService cache, ILogger<CachingBehavior<TRequest, TResponse>> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        // Solo actúa en queries que optan por ser cacheadas
        if (request is not ICacheableQuery cacheableQuery)
            return await next();

        var cacheKey = cacheableQuery.CacheKey;

        // ── Cache HIT ──────────────────────────────────────────────────────────
        var cached = _cache.Get<TResponse>(cacheKey);
        if (cached is not null)
        {
            _logger.LogDebug("Cache HIT para {CacheKey}", cacheKey);
            return cached;
        }

        // ── Cache MISS: ejecutar el handler y almacenar el resultado ──────────
        _logger.LogDebug("Cache MISS para {CacheKey}. Consultando BD...", cacheKey);

        var response = await next();

        if (response is not null)
            _cache.Set(cacheKey, response, cacheableQuery.CacheDuration);

        return response;
    }
}
