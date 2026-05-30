namespace TaskFlow.Application.Common.Caching;

/// <summary>
/// Marker interface para queries que quieren ser cacheadas automáticamente
/// por el CachingBehavior de MediatR.
///
/// Implementar esta interfaz en una Query es todo lo que se necesita
/// para activar el caché — el handler no necesita ningún cambio.
/// </summary>
public interface ICacheableQuery
{
    /// <summary>
    /// Clave única en el store de caché.
    /// Convención: "{recurso}:{discriminador}", ej. "tags:all" o "clients:page:1:size:20".
    /// </summary>
    string CacheKey { get; }

    /// <summary>
    /// Duración absoluta del caché. Después de este tiempo la entrada expira
    /// y el siguiente request va a la base de datos.
    /// </summary>
    TimeSpan CacheDuration { get; }
}
