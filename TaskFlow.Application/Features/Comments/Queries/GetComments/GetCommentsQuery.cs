using MediatR;
using TaskFlow.Application.DTOs.Comments;
using TaskFlow.Shared.Common;

namespace TaskFlow.Application.Features.Comments.Queries.GetComments;

public sealed record GetCommentsQuery(
    Guid TaskItemId,
    int PageNumber = 1,
    int PageSize = 20) : IRequest<Result<PaginatedList<CommentDto>>>;
