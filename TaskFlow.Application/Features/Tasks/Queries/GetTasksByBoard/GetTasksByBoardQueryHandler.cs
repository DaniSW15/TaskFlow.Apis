using MediatR;
using TaskFlow.Application.DTOs.Tasks;
using TaskFlow.Domain.Interfaces;
using TaskFlow.Shared.Common;

namespace TaskFlow.Application.Features.Tasks.Queries.GetTasksByBoard;

public sealed class GetTasksByBoardQueryHandler : IRequestHandler<GetTasksByBoardQuery, Result<PaginatedList<TaskDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetTasksByBoardQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<PaginatedList<TaskDto>>> Handle(GetTasksByBoardQuery request, CancellationToken cancellationToken)
    {
        var board = await _unitOfWork.Boards.GetByIdAsync(request.BoardId, cancellationToken);

        if (board is null)
            return Result.Failure<PaginatedList<TaskDto>>(Error.NotFound);

        if (board.OwnerId != request.RequestingUserId)
            return Result.Failure<PaginatedList<TaskDto>>(Error.Forbidden);

        var (tasks, total) = await _unitOfWork.Tasks.GetByBoardIdPagedAsync(
            request.BoardId, request.PageNumber, request.PageSize, cancellationToken);

        var dtos = tasks.Select(t => new TaskDto(
            t.Id,
            t.Title,
            t.Description,
            t.Status,
            t.Priority,
            t.DueDate,
            t.BoardId,
            t.AssigneeId,
            t.Assignee?.FullName,
            t.CreatedAt,
            t.UpdatedAt)).ToList();

        return Result.Success(new PaginatedList<TaskDto>(dtos, total, request.PageNumber, request.PageSize));
    }
}
