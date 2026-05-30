namespace TaskFlow.Application.Common.Caching;

/// <summary>
/// Abstracción del servicio de caché para que Application no dependa
/// directamente de IMemoryCache (Microsoft.Extensions.Caching.Memory).
/// La implementación concreta vive en Infrastructure.
/// </summary>
public interface ICacheService
{
    /// <summary>Intenta recuperar un valor del caché. Retorna null si no existe.</summary>
    T? Get<T>(string key);

    /// <summary>Almacena un valor en el caché con expiración absoluta.</summary>
    void Set<T>(string key, T value, TimeSpan duration);

    /// <summary>
    /// Elimina una entrada del caché por su clave.
    /// Llamar desde los CommandHandlers al mutar datos cacheados.
    /// </summary>
    void Remove(string key);
}
