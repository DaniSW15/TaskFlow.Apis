using MediatR;
using TaskFlow.Application.DTOs.Tasks;
using TaskFlow.Shared.Common;

namespace TaskFlow.Application.Features.Tasks.Queries.GetTasksByBoard;

public sealed record GetTasksByBoardQuery(
    Guid BoardId,
    Guid RequestingUserId,
    int PageNumber = 1,
    int PageSize = 50) : IRequest<Result<PaginatedList<TaskDto>>>;
