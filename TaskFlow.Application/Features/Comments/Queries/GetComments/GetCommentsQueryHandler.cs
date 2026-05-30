using MediatR;
using TaskFlow.Application.DTOs.Comments;
using TaskFlow.Domain.Interfaces;
using TaskFlow.Shared.Common;

namespace TaskFlow.Application.Features.Comments.Queries.GetComments;

public sealed class GetCommentsQueryHandler : IRequestHandler<GetCommentsQuery, Result<PaginatedList<CommentDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetCommentsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<PaginatedList<CommentDto>>> Handle(GetCommentsQuery request, CancellationToken cancellationToken)
    {
        var (comments, total) = await _unitOfWork.Comments.GetByTaskIdPagedAsync(
            request.TaskItemId, request.PageNumber, request.PageSize, cancellationToken);

        var dtos = comments.Select(c => new CommentDto(
            c.Id,
            c.Content,
            c.TaskItemId,
            c.AuthorId,
            c.Author.FullName,
            c.CreatedAt)).ToList();

        return Result.Success(new PaginatedList<CommentDto>(dtos, total, request.PageNumber, request.PageSize));
    }
}
