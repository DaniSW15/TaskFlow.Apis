using MediatR;
using TaskFlow.Application.DTOs.Tags;
using TaskFlow.Domain.Interfaces;
using TaskFlow.Shared.Common;

namespace TaskFlow.Application.Features.Tags.Queries.GetTags;

public sealed class GetTagsQueryHandler : IRequestHandler<GetTagsQuery, Result<IReadOnlyList<TagDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetTagsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<IReadOnlyList<TagDto>>> Handle(GetTagsQuery request, CancellationToken cancellationToken)
    {
        var tags = await _unitOfWork.Tags.GetAllAsync(cancellationToken);

        var dtos = tags
            .Select(t => new TagDto(t.Id, t.Name, t.Color))
            .ToList();

        return Result.Success<IReadOnlyList<TagDto>>(dtos);
    }
}
