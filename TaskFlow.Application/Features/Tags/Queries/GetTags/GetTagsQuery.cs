using MediatR;
using TaskFlow.Application.Common.Caching;
using TaskFlow.Application.DTOs.Tags;
using TaskFlow.Shared.Common;

namespace TaskFlow.Application.Features.Tags.Queries.GetTags;

/// <summary>
/// Implementa ICacheableQuery: el CachingBehavior cacheará automáticamente
/// el resultado durante 10 minutos. No se necesita ningún cambio en el handler.
/// </summary>
public sealed record GetTagsQuery : IRequest<Result<IReadOnlyList<TagDto>>>, ICacheableQuery
{
    public string CacheKey => "tags:all";
    public TimeSpan CacheDuration => TimeSpan.FromMinutes(10);
}
